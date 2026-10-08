import '../App.css';
import { useLocation, useNavigate, NavLink, Link} from 'react-router-dom';
import { useEffect, useState } from 'react';
import { logoutUser } from '../api/user-api';

function Header() {
  const location = useLocation();
  const navigate = useNavigate();
  const [title, setTitle] = useState('Components Monitoring Dashboard');
  
  // State to hold user info
  const [userInfo, setUserInfo] = useState({ username: '', fullName: '' });

  useEffect(() => {
    // Retrieve info from localStorage when component mounts or location changes
    const storedUsername = localStorage.getItem('username');
    const storedFullName = localStorage.getItem('fullName');
    
    if (storedUsername && storedFullName) {
      setUserInfo({ username: storedUsername, fullName: storedFullName });
    }
  }, [location]);

  const handleLogout = async () => {
    try {
      await logoutUser(); 
    } catch (err) {
      console.error('Error during logout:', err);
    } finally {
      localStorage.clear();
      setUserInfo({ username: '', fullName: '' }); // Clear local state
      navigate('/');
    }
  };

  const isLoginPage = location.pathname === '/';


  useEffect(() => {
  const fixHeaderWidth = () => {
    const header = document.querySelector('.main-header');
    if (header) {
      // Reset width first to get an accurate measurement of the body
      header.style.minWidth = '100%';
      
      // Calculate the total scrollable width of the page
      const scrollWidth = document.documentElement.scrollWidth;
      
      // Apply that width to the header
      header.style.minWidth = `${scrollWidth}px`;
    }
  };

  // Run on mount
  fixHeaderWidth();

  // Run whenever the window is resized or zoomed
  window.addEventListener('resize', fixHeaderWidth);
  
  return () => window.removeEventListener('resize', fixHeaderWidth);
}, []);

  return (
    <header className="main-header" style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 2rem',
      height: '70px',
      backgroundColor: '#fff',
      boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
      width: '100%'
    }}>
      
      {/* Branding Section */}
      <div className="header-branding" style={{ flex: '1', display: 'flex', alignItems: 'center' }}>
        {!isLoginPage ? (
          <Link to="/Applications" style={{ textDecoration: 'none', color: 'inherit' }}>
            <h1 style={{ margin: 0, fontSize: '1.5rem', cursor: 'pointer' }}>{title}</h1>
          </Link>
        ) : (
          <h1 style={{ margin: 0, fontSize: '1.5rem' }}>{title}</h1>
        )}
      </div>

      {/* Navigation Section */}
      {!isLoginPage && (
        <nav className="header-nav" style={{ display: 'flex', gap: '30px', flexShrink: 0, padding: '0 20px' }}>
          <NavLink to="/Applications" style={({ isActive }) => ({ fontWeight: isActive ? 'bold' : 'normal' })}>Applications</NavLink>
          <NavLink to="/ApplicationGroups" style={({ isActive }) => ({ fontWeight: isActive ? 'bold' : 'normal' })}>Application Groups</NavLink>
          <NavLink to="/Exceptions"style={({ isActive }) => ({ fontWeight: isActive ? 'bold' : 'normal' })}>Exceptions</NavLink> 
          <NavLink to="/User" style={({ isActive }) => ({ fontWeight: isActive ? 'bold' : 'normal' })}>Users</NavLink>
        </nav>
      )}

      {/* Actions Section (The User Name is added here) */}
      {!isLoginPage ? (
        <div className="header-actions" style={{ 
          flex: '1', 
          display: 'flex', 
          justifyContent: 'flex-end', 
          alignItems: 'center', // Align text and button vertically
          gap: '15px', 
          flexShrink: 0 
        }}>
          {/* Displaying the format: [Username] [First Name] */}
          <span style={{ fontSize: '1rem', color: '#ffffff', fontWeight: '500' }}>
            {userInfo.username} ({userInfo.fullName.split(' ')[0]})
          </span>
          
          <button onClick={handleLogout} className="logout-button" style={{ whiteSpace: 'nowrap' }}>
            Logout
          </button>
        </div>
      ) : (
        <div style={{ flex: 1 }}></div>
      )}
    </header>
  );
}

export default Header;









