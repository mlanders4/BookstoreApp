import React, { useContext } from "react";
import { FiX } from "react-icons/fi";
import { CartContext } from "./CartContext";
import "./CartSidebar.css";

function CartSidebar() {
  const { cartItems, setCartItems, setCartOpen } = useContext(CartContext);

  const handleQuantity = (isbn, delta) => {
    setCartItems((prev) =>
      prev.map((item) =>
        item.isbn === isbn
          ? { ...item, quantity: Math.max(1, item.quantity + delta) }
          : item
      )
    );
  };

  const handleRemove = (isbn) => {
    setCartItems((prev) => prev.filter((item) => item.isbn !== isbn));
  };

  const total = cartItems.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  return (
    <div className="cart-sidebar">
      <div className="cart-header">
        <button className="close-cart" onClick={() => setCartOpen(false)}>
        <FiX size={20} />
        </button>
      </div>
      <div className="cart-body">
        {cartItems.map((item) => (
          <div key={item.isbn} className="cart-item">
            <img src={`/images/${item.product_Image}`} alt={item.name} />
            <div className="info">
              <h2>{item.name}</h2>
              <h3>{item.author}</h3>
              <h4>{item.category}</h4>
              <p>${item.price.toFixed(2)}</p>
              <div className="info-bottom">
                <div className="quantity-controls">
                  <button onClick={() => handleQuantity(item.isbn, -1)}>
                    -
                  </button>
                  <span>{item.quantity}</span>
                  <button onClick={() => handleQuantity(item.isbn, 1)}>
                    +
                  </button>
                </div>
                <button
                  className="remove-btn"
                  onClick={() => handleRemove(item.isbn)}
                >
                  Remove
                </button>
              </div>
            </div>
          </div>
        ))}
        <div className="checkout">
          <h3>Subtotal: ${total.toFixed(2)}</h3>
          <h3 className="savings">
            Savings: <span className="highlight-price">-$0.00</span>
          </h3>
          <h3>Estimated total: ${total.toFixed(2)}</h3>
          <button className="checkout-btn">Continue to checkout</button>
        </div>
      </div>
    </div>
  );
}

export default CartSidebar;
