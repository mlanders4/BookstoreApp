import React, { useState } from 'react';
import axios from '../api/axios';
import '../style.css';

function SignUp({ switchToLogin }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [login, setLogin] = useState('');
  const [first_name, setFirst_name] = useState('');
  const [last_name, setLast_name] = useState('');
  const [phone_n, setPhone_n] = useState('');

  const handleSignUp = async (e) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      alert('Passwords do not match!');
      return;
    }

    try {
      const response = await axios.post('/auth/signup', {
        Email: email,
        Password: password,
        Login: login,
        FirstName: first_name,
        LastName: last_name,
        PhoneN: phone_n
      });
      
      

      console.log('Signup success:', response.data);
      alert('Account created! Please log in.');
      switchToLogin();
    } catch (error) {
      console.error('Signup failed:', error.response ? error.response.data : error.message);
      alert('Signup failed. Try again.');
    }
  };

  return (
    <div className="form-box register">
      <h2>Register</h2>
      <form onSubmit={handleSignUp}>
        <div className="input-box">
          <input
            type="text"
            required
            value={login}
            onChange={(e) => setLogin(e.target.value)}
          />
          <label>Login</label>
        </div>

        <div className="input-box">
          <input
            type="text"
            required
            value={first_name}
            onChange={(e) => setFirst_name(e.target.value)}
          />
          <label>First Name</label>
        </div>

        <div className="input-box">
          <input
            type="text"
            required
            value={last_name}
            onChange={(e) => setLast_name(e.target.value)}
          />
          <label>Last Name</label>
        </div>

        <div className="input-box">
          <input
            type="text"
            required
            value={phone_n}
            onChange={(e) => setPhone_n(e.target.value)}
          />
          <label>Phone Number</label>
        </div>

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

        <div className="input-box">
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
          />
          <label>Confirm Password</label>
        </div>

        <button type="submit" className="btn">Register</button>

        <div className="login-register">
          <p>
            Already have an account?{' '}
            <button type="button" className="link-button" onClick={switchToLogin}>
              Login
            </button>
          </p>
        </div>
      </form>
    </div>
  );
}

export default SignUp;
