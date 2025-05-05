import React from "react";
import { useNavigate } from "react-router-dom";

function LoginPage() {
  const navigate = useNavigate();

  const handleLogin = () => {
    // You’d typically verify credentials here
    navigate("/catalog"); // redirect to catalog
  };

  return (
    <div className="login-page">
      <h2>Login</h2>
      <button onClick={handleLogin}>Enter</button>
    </div>
  );
}

export default LoginPage;