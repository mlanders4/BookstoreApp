import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./pages/Catalog/Navbar";
import CatalogPage from "./pages/Catalog/CatalogPage";
import CartSidebar from "./pages/Catalog/CartSidebar";
import { CartContext } from "./pages/Catalog/CartContext";
import LoginPage from "./pages/Login/LoginPage"; // <-- your login component

function App() {
  const [cartItems, setCartItems] = useState([]);
  const [cartOpen, setCartOpen] = useState(false);

  return (
    <CartContext.Provider value={{ cartItems, setCartItems, cartOpen, setCartOpen }}>
      <Router>
        <Navbar />
        <Routes>
          <Route path="/" element={<LoginPage />} />
          <Route path="/catalog" element={<CatalogPage />} />
        </Routes>
        {cartOpen && <CartSidebar />}
      </Router>
    </CartContext.Provider>
  );
}

export default App;