import React, { useState, useEffect, useContext } from "react";
import "./CatalogPage.css";
import { CartContext } from "./CartContext";
import { FiX } from "react-icons/fi";

function CatalogPage() {
  const [books, setBooks] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedGenre, setSelectedGenre] = useState("");
  const [selectedPrice, setSelectedPrice] = useState("");
  const [selectedRating, setSelectedRating] = useState("");

  useEffect(() => {
    fetch("http://localhost:5246/api/catalog")
      .then((res) => res.json())
      .then((data) => {
        console.log("Books fetched:", data); 
        setBooks(data);
      })
      .catch((err) => console.error("Error fetching books:", err));
  }, []);

  const { cartItems, setCartItems } = useContext(CartContext);

  // Add this function inside CatalogPage component
  const handleAddToCart = (book) => {
    setCartItems((prevItems) => {
      const existing = prevItems.find((item) => item.isbn === book.isbn);
      if (existing) {
        return prevItems.map((item) =>
          item.isbn === book.isbn
            ? { ...item, quantity: item.quantity + 1 }
            : item
        );
      } else {
        return [...prevItems, { ...book, quantity: 1 }];
      }
    });
  };

  // 🔍 Filter books using search + dropdowns
  const filteredBooks = books.filter((book) => {
    const matchesTitle = book.name
      .toLowerCase()
      .includes(searchQuery.toLowerCase());
    const matchesGenre =
      selectedGenre === "" || book.category === selectedGenre;
    const matchesPrice =
      selectedPrice === "" || book.price <= parseFloat(selectedPrice);
    // Note: Backend doesn't include rating yet, so this is placeholder
    const matchesRating =
      selectedRating === "" || 4.5 >= parseFloat(selectedRating); // placeholder logic

    return matchesTitle && matchesGenre && matchesPrice && matchesRating;
  });

  const handleRemove = (isbn) => {
    setCartItems((prevItems) => prevItems.filter((item) => item.isbn !== isbn));
  };

  const [selectedBook, setSelectedBook] = useState(null);

  return (
    <div id="catalog">
      <div className="search-filter">
        <input
          type="text"
          placeholder="Search by Title"
          className="search-bar"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
        <div className="filters">
          <select
            value={selectedGenre}
            onChange={(e) => setSelectedGenre(e.target.value)}
          >
            <option value="">Any Genres</option>
            <option value="Fiction">Fiction</option>
            <option value="Epic">Epic</option>
            <option value="Philosophical">Philosophical</option>
            <option value="Romance">Romance</option>
            <option value="Adventure">Adventure</option>
            <option value="Horror">Horror</option>
          </select>

          <select
            value={selectedPrice}
            onChange={(e) => setSelectedPrice(e.target.value)}
          >
            <option value="">Any Price</option>
            <option value="10">Under $10</option>
            <option value="15">Under $15</option>
            <option value="20">Under $20</option>
          </select>

          <select
            value={selectedRating}
            onChange={(e) => setSelectedRating(e.target.value)}
          >
            <option value="">Any Rating</option>
            <option value="4">Above 3.0</option>
            <option value="4.5">Above 4.0</option>
            <option value="5">5.0 Only</option>
          </select>
        </div>
      </div>

      <div className="padding-books">
        <div id="main" className="book-grid">
          {filteredBooks.map((book) => (
            <div key={book.isbn} className="book-card">
              <img
                src={`/images/${book.product_Image}`}
                alt={book.name}
                className="clickable-image"
                onClick={() => setSelectedBook(book)}
              />
              <h3 className="book-title" onClick={() => setSelectedBook(book)}>
                {book.name.toUpperCase()}
              </h3>
              <p className="book-author">
                By <strong>{book.author}</strong>
              </p>
              {/* <p className="book-genre">Genre: {book.category}</p>
              <p className="book-rating">N/A</p> 
              <p className="learn-more" onClick={() => setSelectedBook(book)}>
                Learn More
              </p> */}
              <p className="price">
                ${book.price.toFixed(2)}{" "}
                {cartItems.some((item) => item.isbn === book.isbn) ? (
                  <span
                    className="plus"
                    onClick={() => handleRemove(book.isbn)}
                  >
                    -
                  </span>
                ) : (
                  <span className="plus" onClick={() => handleAddToCart(book)}>
                    +
                  </span>
                )}
              </p>
            </div>
          ))}
        </div>
        {selectedBook && (
          <div className="popup-overlay" onClick={() => setSelectedBook(null)}>
            <div className="popup-content" onClick={(e) => e.stopPropagation()}>
              <button
                className="close-popup"
                onClick={() => setSelectedBook(null)}
              >
                <FiX size={20} />
              </button>
              <div className="popup-body">
                <div className="col-1">
                  <img
                    src={`/images/${selectedBook.product_Image}`}
                    alt={selectedBook.name}
                  />
                </div>
                <div className="col-2">
                  <div className="row-0">
                    <h2>{selectedBook.name.toUpperCase()}</h2>
                    <p>
                      <strong>${selectedBook.price.toFixed(2)} </strong>

                      {cartItems.some(
                        (item) => item.isbn === selectedBook.isbn
                      ) ? (
                        <span
                          className="plus"
                          onClick={() => handleRemove(selectedBook.isbn)}
                        >
                          -
                        </span>
                      ) : (
                        <span
                          className="plus"
                          onClick={() => handleAddToCart(selectedBook)}
                        >
                          +
                        </span>
                      )}
                    </p>
                  </div>
                  <div className="row-1">
                    <p>
                      Author: <strong>{selectedBook.author}</strong>
                    </p>
                    <p>
                      Genre: <strong>{selectedBook.category}</strong>
                    </p>
                  </div>
                  <div className="row-2">
                    <p>
                      {selectedBook.description || "No description available."}
                    </p>
                  </div>
                  <div className="row-3">
                    <p>
                      <p>
                        Sale Status: <strong>{selectedBook.sale_Status}</strong>
                      </p>
                      ISBN: <strong>{selectedBook.isbn}</strong>
                    </p>
                    <p>
                      Dimension: <strong>{selectedBook.dimension}</strong>
                    </p>
                    <p>
                      Weight: <strong>{selectedBook.weight}</strong>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default CatalogPage;
