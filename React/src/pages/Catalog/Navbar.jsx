import React, { useContext } from "react";
import "./Navbar.css";
import { FaUserCircle, FaShoppingCart } from "react-icons/fa";
import { CartContext } from "./CartContext";

function Navbar() {
  const { cartItems, setCartOpen } = useContext(CartContext);
  const totalItems = cartItems.reduce((sum, item) => sum + item.quantity, 0);

  return (
    <div className="navbar">
      <div className="navbar-title-container">
        <div className="navbar-title">BOOK STORE</div>
      </div>
      <div className="navbar-features-container">
        {/* <FaUserCircle size={26} className="navbar-icon" /> */}
        <div className="navbar-icon" onClick={() => setCartOpen(true)} style={{ position: "relative" }}>
          <FaShoppingCart size={26} />
          {totalItems > 0 && <span className="cart-count">{totalItems}</span>}
        </div>
      </div>
    </div>
  );
}

export default Navbar;