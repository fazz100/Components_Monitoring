import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './App.css';
import ApplicationList from './views/application-list';
import ApplicationException from './views/application-exception'; // Import the new view
import Login from './views/login';
import Header from './components/header';
import ProtectedRoute from './components/protected-route';
import User from './views/user';

function App() {
  return (
    <BrowserRouter>
      <Header />
      <div id="divBody">
        <Routes>
          {/* Public Route */}
          <Route path="/" element={<Login />} />

          {/* Protected Routes */}
          <Route
            path="/Applications"
            element={
              // <ProtectedRoute>
                <ApplicationList />
              // </ProtectedRoute>
            }
          />
          
          {/* New Route for Silenced/Exceptions */}
          <Route
            path="/Exceptions"
            element={
              // <ProtectedRoute>
                <ApplicationException />
              // </ProtectedRoute>
            }
          />

          <Route
            path="/User"
            element={
              // <ProtectedRoute>
                <User />
              // </ProtectedRoute>
            }
          />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;