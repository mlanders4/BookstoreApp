import React, { useState } from 'react';
import Login from './components/Login';
import SignUp from './components/SignUp';
import './style.css'; 

function App() {
  const [isLogin, setIsLogin] = useState(true);

  const switchToSignUp = () => setIsLogin(false);
  const switchToLogin = () => setIsLogin(true);

  return (
    <div className="App">
      <header>
        <h2 className="logo">Bookstore</h2>
        <nav className="navigation">
          <a href="#">Home</a>
          <a href="#">About</a>
          <a href="#">Services</a>
          <a href="#">Contact</a>
        </nav>
      </header>

     
      <div className={`wrapper active-popup ${!isLogin ? 'active' : ''}`}>
        <span className="icon-close" onClick={switchToLogin}>
          ✖
        </span>

        {isLogin ? (
          <Login switchToSignUp={switchToSignUp} />
        ) : (
          <SignUp switchToLogin={switchToLogin} />
        )}
      </div>
    </div>
  );
}

export default App;

