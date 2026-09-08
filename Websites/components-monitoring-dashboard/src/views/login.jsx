import React, { useState } from 'react';
import { loginUser } from '../api/user-api';
import { useNavigate } from 'react-router-dom';

function Login() {
  const [Username, setUsername] = useState('');
  const [PasswordString, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    setSuccess(false);

    try {
      const response = await loginUser({ Username, PasswordString });
      
      // Checking for response.Data and IsAuthenticated based on your API structure
      if (response && response.Data && response.Data.IsAuthenticated) {
        // Save token and related info in localStorage
        localStorage.setItem('AuthToken', response.Data.Token);
        localStorage.setItem('TokenExpiry', response.Data.ExpiresAt);
        localStorage.setItem('Username', response.Data.Username);
        localStorage.setItem('UserId', response.Data.UserId);

        localStorage.setItem('token', response.Data.Token);
        localStorage.setItem('userId', response.Data.UserId);
        localStorage.setItem('username', response.Data.Username);
        localStorage.setItem('fullName', response.Data.FullName); // This is [First Name] [Last Name]

        setSuccess(true);

        

        // Delay redirect slightly for user feedback
        setTimeout(() => navigate('/Applications'), 2000);
      } else {
        setError(response.message || 'Invalid username or password.');
      }
    } catch (err) {
      console.error(err);
      setError('An error occurred during login.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      {/* The Header is now handled globally by App.jsx */}
      <div className="login-card">
        <h2>Login</h2>
        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label>Username:</label>
            <input
              type="text"
              value={Username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter username"
              required
            />
          </div>
          <div className="form-group">
            <label>Password:</label>
            <input
              type="password"
              value={PasswordString}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              required
            />
          </div>
          <button type="submit" className="login-btn" disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        {error && <p style={{ color: '#d9534f', marginTop: '10px', fontWeight: 'bold' }}>{error}</p>}
        {success && <p style={{ color: '#5cb85c', marginTop: '10px', fontWeight: 'bold' }}>Login successful! Redirecting...</p>}
      </div>
    </div>
  );
}

export default Login;
