import React, { useState } from "react";
import Navbar from "./pages/Catalog/Navbar";
import CatalogPage from "./pages/Catalog/CatalogPage";
import CartSidebar from "./pages/Catalog/CartSidebar";
import { CartContext } from "./pages/Catalog/CartContext";

function App() {
  const [cartItems, setCartItems] = useState([]);
  const [cartOpen, setCartOpen] = useState(false);

  return (
    <CartContext.Provider value={{ cartItems, setCartItems, cartOpen, setCartOpen }}>
      <Navbar />
      <CatalogPage />
      {cartOpen && <CartSidebar />}
    </CartContext.Provider>
  );
}

export default App;