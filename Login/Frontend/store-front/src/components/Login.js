import React, { useState } from 'react';
import axios from '../api/axios'; 
import '../style.css';            

function Login({ switchToSignUp }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post('/auth/login', {
        Email: email,
        Password: password
      });
      
      console.log('Login success:', response.data);
      alert('Login successful!');
      // Later: save token or navigate
    } catch (error) {
      console.error('Login failed:', error.response ? error.response.data : error.message);
      alert('Login failed. Check email and password.');
    }
  };

  return (
    <div className="form-box login">
      <h2>Login</h2>
      <form onSubmit={handleLogin}>
        <div className="input-box">
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <label>Email</label>
        </div>

        <div className="input-box">
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <label>Password</label>
        </div>

        <div className="remember-forgot">
          <label>
            <input type="checkbox" /> Remember me
          </label>
          <a href="#">Forgot Password?</a>
        </div>

        <button type="submit" className="btn">Login</button>

        <div className="login-register">
          <p>
            Don't have an account?{' '}
            <button type="button" className="link-button" onClick={switchToSignUp}>
              Register
            </button>
          </p>
        </div>
      </form>
    </div>
  );
}

export default Login;
