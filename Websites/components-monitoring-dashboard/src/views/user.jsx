import React, { useEffect, useState, useMemo } from 'react';
import { createUser, getUsers, updateUserDetails, updateUserStatus, changeUserPassword } from '../api/user-api';
import { useNavigate } from 'react-router-dom';
import { createPortal } from 'react-dom';
import '../App.css';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEdit, faToggleOn, faToggleOff, faKey } from '@fortawesome/free-solid-svg-icons';

function User() {
    const navigate = useNavigate();
    const [users, setUsers] = useState([]);
    const [showAddModal, setShowAddModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [showPasswordModal, setShowPasswordModal] = useState(false);

    const [searchTerm, setSearchTerm] = useState('');     
    const [searchQuery, setSearchQuery] = useState('');   

    const [formData, setFormData] = useState({
        Username: '',
        PasswordString: '',
        FirstName: '',
        LastName: ''
    });

    const [editData, setEditData] = useState({
        Id: null,
        FirstName: '',
        LastName: '',
        Username: '',
        IsDeleted: false
    });

    const [passwordData, setPasswordData] = useState({
        Id: null,
        newPassword: '',
        confirmPassword: ''
    });

    const [message, setMessage] = useState('');
    const [messageColor, setMessageColor] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    // 1. New state for sorting
    const [sortConfig, setSortConfig] = useState({ key: 'Username', direction: 'asc' });

    // 2. Sorting Logic
    const sortedUsers = useMemo(() => {
        let sortableItems = [...users];
        if (sortConfig.key !== null) {
            sortableItems.sort((a, b) => {
                let aValue = a[sortConfig.key];
                let bValue = b[sortConfig.key];

                // 1. Handle Nulls/Undefined
                if (aValue === null || aValue === undefined) aValue = '';
                if (bValue === null || bValue === undefined) bValue = '';

                // 2. Boolean Handling (Status)
                if (sortConfig.key === 'IsDeleted') {
                    return sortConfig.direction === 'asc' 
                        ? (aValue === bValue ? 0 : aValue ? 1 : -1) 
                        : (aValue === bValue ? 0 : aValue ? -1 : 1);
                }

                // 3. String/Alphabetical Handling (Username, Names, etc.)
                // Convert to string and use localeCompare for proper alphabetical order
                const stringA = aValue.toString();
                const stringB = bValue.toString();

                const comparison = stringA.localeCompare(stringB, undefined, {
                    sensitivity: 'base', // Ignores case and accents (a = A)
                    numeric: true        // Sorts numbers naturally (User2 < User10)
                });

                return sortConfig.direction === 'asc' ? comparison : -comparison;
            });
        }
        return sortableItems;
    }, [users, sortConfig]);

    // 3. Request Sort Function
    const requestSort = (key) => {
        let direction = 'asc';
        if (sortConfig.key === key && sortConfig.direction === 'asc') {
            direction = 'desc';
        }
        setSortConfig({ key, direction });
    };

    const getSortIcon = (name) => {
        if (sortConfig.key !== name) return '↕️';
        return sortConfig.direction === 'asc' ? '🔼' : '🔽';
    };

    async function fetchUsers() {
        try {
            const res = await getUsers(searchQuery);
            setUsers(res?.Data || []);
        } catch (error) {
            console.error('Failed to fetch users:', error);
        }
    }

    useEffect(() => {
        fetchUsers();
    }, [searchQuery]);

    const handleSearch = () => {
        setSearchQuery(searchTerm); // This triggers the useEffect above
    };
    
    const handleAddChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleAddSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setIsSubmitting(true);
        try {
            const payload = {
                ...formData,
                CreatedDate: new Date().toISOString(),
                CreatedBy: 1,
                UpdatedDate: new Date().toISOString(),
                UpdatedBy: 1,
                IsDeleted: false,
            };
            const result = await createUser(payload);
            if (result && result.Success !== false) {
                setMessage('User successfully created!');
                setMessageColor('green');
                setFormData({ Username: '', PasswordString: '', FirstName: '', LastName: '' });
                await fetchUsers();
                setTimeout(() => setShowAddModal(false), 1500);
            } else {
                setMessage('Failed to create user.');
                setMessageColor('red');
            }
        } catch (error) {
            setMessage(`Error: ${error.message}`);
            setMessageColor('red');
        } finally {
            setIsSubmitting(false);
        }
    };

    const openEditModal = (user) => {
        setEditData({
            Id: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Username: user.Username,
            IsDeleted: user.IsDeleted,
        });
        setMessage('');
        setShowEditModal(true);
    };

    const handleEditChange = (e) => {
        setEditData({ ...editData, [e.target.name]: e.target.value });
    };

    const handleEditSubmit = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            const payload = {
                Id: editData.Id,
                FirstName: editData.FirstName,
                LastName: editData.LastName,
                Username: editData.Username,
                UpdatedDate: new Date().toISOString(),
                UpdatedBy: 1,
                IsDeleted: editData.IsDeleted,
            };
            const result = await updateUserDetails(payload);
            if (result && result.Success !== false) {
                await fetchUsers();
                setShowEditModal(false);
            }
        } catch (error) {
            console.error('Error updating user:', error);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Updated Toggle logic with prompt and feedback
    const handleToggleActive = async (user) => {
        const action = user.IsDeleted ? "activate" : "deactivate";
        if (!window.confirm(`Are you sure you want to ${action} user: ${user.Username}?`)) return;

        try {
            const res = await updateUserStatus(user.Id, !user.IsDeleted, user.Username);
            if (res && res.Success !== false) {
                alert(`User ${user.Username} ${user.IsDeleted ? 'activated' : 'deactivated'} successfully.`);
                await fetchUsers();
            } else {
                alert("Failed to update user status.");
            }
        } catch (error) {
            console.error('Error toggling user status:', error);
            alert("Error: " + error.message);
        }
    };

    const openPasswordModal = (user) => {
        setPasswordData({ Id: user.Id, newPassword: '', confirmPassword: '' });
        setMessage('');
        setShowPasswordModal(true);
    };

    const handlePasswordChange = (e) => {
        setPasswordData({ ...passwordData, [e.target.name]: e.target.value });
    };

    const handlePasswordSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setIsSubmitting(true);

        if (passwordData.newPassword !== passwordData.confirmPassword) {
            setMessage('Passwords do not match.');
            setMessageColor('red');
            setIsSubmitting(false);
            return;
        }

        try {
            await changeUserPassword(passwordData.Id, passwordData.newPassword);
            setMessage('Password changed successfully!');
            setMessageColor('green');
            setTimeout(() => {
                setShowPasswordModal(false);
                setMessage('');
            }, 1500);
        } catch (error) {
            setMessage('Failed to change password.');
            setMessageColor('red');
        } finally {
            setIsSubmitting(false);
        }
    };

    // --- Modal Portals (Cleaned for App.css alignment) ---

    const addModal = showAddModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content">
                <h2>Add New User</h2>
                <form onSubmit={handleAddSubmit}>
                    <div className="form-group">
                        <label>Username:</label>
                        <input type="text" name="Username" value={formData.Username} onChange={handleAddChange} required />
                    </div>
                    <div className="form-group">
                        <label>Password:</label>
                        <input type="password" name="PasswordString" value={formData.PasswordString} onChange={handleAddChange} required />
                    </div>
                    <div className="form-group">
                        <label>First Name:</label>
                        <input type="text" name="FirstName" value={formData.FirstName} onChange={handleAddChange} required />
                    </div>
                    <div className="form-group">
                        <label>Last Name:</label>
                        <input type="text" name="LastName" value={formData.LastName} onChange={handleAddChange} required />
                    </div>
                    <div style={{ marginTop: '10px' }}>
                        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating...' : 'Create User'}</button>
                        <button type="button" onClick={() => setShowAddModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                    </div>
                    {message && <p style={{ color: messageColor, marginTop: '1rem', textAlign: 'center', fontWeight: 'bold' }}>{message}</p>}
                </form>
            </div>
        </div>, document.body
    ) : null;

    const editModal = showEditModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content">
                <h2>Edit User</h2>
                <form onSubmit={handleEditSubmit}>
                    <div className="form-group">
                        <label>First Name:</label>
                        <input type="text" name="FirstName" value={editData.FirstName} onChange={handleEditChange} required />
                    </div>
                    <div className="form-group">
                        <label>Last Name:</label>
                        <input type="text" name="LastName" value={editData.LastName} onChange={handleEditChange} required />
                    </div>
                    <div style={{ marginTop: '10px' }}>
                        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Updating...' : 'Update User'}</button>
                        <button type="button" onClick={() => setShowEditModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                    </div>
                </form>
            </div>
        </div>, document.body
    ) : null;

    const passwordModal = showPasswordModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content">
                <h2>Change Password</h2>
                <form onSubmit={handlePasswordSubmit}>
                    <div className="form-group">
                        <label>New Password:</label>
                        <input type="password" name="newPassword" value={passwordData.newPassword} onChange={handlePasswordChange} required />
                    </div>
                    <div className="form-group">
                        <label>Confirm New Password:</label>
                        <input type="password" name="confirmPassword" value={passwordData.confirmPassword} onChange={handlePasswordChange} required />
                    </div>
                    <div style={{ marginTop: '10px' }}>
                        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Updating...' : 'Change Password'}</button>
                        <button type="button" onClick={() => setShowPasswordModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                    </div>
                    {message && <p style={{ color: messageColor, marginTop: '1rem', textAlign: 'center', fontWeight: 'bold' }}>{message}</p>}
                </form>
            </div>
        </div>, document.body
    ) : null;

    return (
        <div style={{ padding: '20px 0px' }} >
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }} >
                <h2 style={{ margin: 0 }}>User Management</h2>
                <div style={{ display: 'flex', gap: '5px', marginLeft: '10px' }}>
                    <input 
                        type="text" 
                        placeholder="Search by Userame or Name..." 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleSearch()} // Allow "Enter" key to search
                        style={{ 
                            padding: '8px 12px', 
                            borderRadius: '4px', 
                            border: '1px solid #ccc',
                            width: '500px' 
                        }}
                    />
                    <button 
                        type="button" 
                        onClick={handleSearch}
                        style={{ padding: '8px 15px' }}
                    >
                        Search
                    </button>
                    <button 
                        type="button" 
                        onClick={() => { setSearchTerm(''); setSearchQuery(''); }}
                        style={{ padding: '8px 15px', backgroundColor: '#eee', color: '#555' }}
                    >
                        Clear
                    </button>
                </div>
                <button onClick={() => { setMessage(''); setShowAddModal(true); }}>Add User</button>
            </div>

            {users && users.length > 0 ? (
                <table>
                    <thead>
                        <tr>
                            {/* 4. Make headers clickable */}
                            <th onClick={() => requestSort('Username')} style={{ cursor: 'pointer' }}>
                                Username
                            </th>
                            <th onClick={() => requestSort('FirstName')} style={{ cursor: 'pointer' }}>
                                First Name
                            </th>
                            <th onClick={() => requestSort('LastName')} style={{ cursor: 'pointer' }}>
                                Last Name
                            </th>
                            <th onClick={() => requestSort('CreatedDate')} style={{ cursor: 'pointer' }}>
                                Created Date
                            </th>
                            <th onClick={() => requestSort('UpdatedDate')} style={{ cursor: 'pointer' }}>
                                Updated Date
                            </th>
                            <th onClick={() => requestSort('IsDeleted')} style={{ cursor: 'pointer', textAlign: 'center' }}>
                                Status
                            </th>
                            <th style={{ textAlign: 'center' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {/* 5. Map over sortedUsers instead of users */}
                        {sortedUsers.map((user) => (
                            <tr key={user.Id}>
                                <td>{user.Username}</td>
                                <td>{user.FirstName}</td>
                                <td>{user.LastName}</td>
                                <td>{new Date(user.CreatedDate).toLocaleDateString()}</td>
                                <td>{new Date(user.UpdatedDate).toLocaleDateString()}</td>
                                <td style={{ textAlign: 'center' }}>
                                    {!user.IsDeleted ? 
                                        <span style={{ color: 'green', fontWeight: 'bold' }}>Active</span> : 
                                        <span style={{ color: 'red' }}>Inactive</span>
                                    }
                                </td>
                                <td style={{ textAlign: 'center' }}>
                                  {/* Edit User Icon */}
                                  <FontAwesomeIcon 
                                      icon={faEdit} 
                                      title={user.IsDeleted ? "Activate user to edit" : "Edit User"} 
                                      style={{ 
                                          marginRight: '15px', 
                                          cursor: user.IsDeleted ? 'not-allowed' : 'pointer', 
                                          color: user.IsDeleted ? '#bdc3c7' : 'inherit', // Muted grey if deleted
                                          opacity: user.IsDeleted ? 0.6 : 1
                                      }} 
                                      onClick={() => !user.IsDeleted && openEditModal(user)} // Guard clause
                                  />

                                  {/* Toggle Active Icon (Always enabled so you can reactivate) */}
                                  <FontAwesomeIcon 
                                      icon={!user.IsDeleted ? faToggleOn : faToggleOff} 
                                      title={!user.IsDeleted ? 'Deactivate' : 'Activate'} 
                                      style={{ 
                                          color: !user.IsDeleted ? '#2ecc71' : '#bdc3c7', 
                                          cursor: 'pointer', 
                                          marginRight: '15px', 
                                          fontSize: '1.2rem' 
                                      }} 
                                      onClick={() => handleToggleActive(user)} 
                                  />

                                  {/* Change Password Icon */}
                                  <FontAwesomeIcon 
                                      icon={faKey} 
                                      title={user.IsDeleted ? "Activate user to change password" : "Change Password"} 
                                      style={{ 
                                          cursor: user.IsDeleted ? 'not-allowed' : 'pointer', 
                                          color: user.IsDeleted ? '#bdc3c7' : '#f39c12', // Muted grey if deleted
                                          opacity: user.IsDeleted ? 0.6 : 1
                                      }} 
                                      onClick={() => !user.IsDeleted && openPasswordModal(user)} // Guard clause
                                  />
                              </td>
                                {/* <td style={{ textAlign: 'center' }}>
                                    <FontAwesomeIcon icon={faEdit} title="Edit User" style={{ marginRight: '15px', cursor: 'pointer' }} onClick={() => openEditModal(user)} />
                                    <FontAwesomeIcon 
                                        icon={!user.IsDeleted ? faToggleOn : faToggleOff} 
                                        title={!user.IsDeleted ? 'Deactivate' : 'Activate'} 
                                        style={{ color: !user.IsDeleted ? '#2ecc71' : '#bdc3c7', cursor: 'pointer', marginRight: '15px', fontSize: '1.2rem' }} 
                                        onClick={() => handleToggleActive(user)} 
                                    />
                                    <FontAwesomeIcon icon={faKey} title="Change Password" style={{ cursor: 'pointer', color: '#f39c12' }} onClick={() => openPasswordModal(user)} />
                                </td> */}
                            </tr>
                        ))}
                    </tbody>
                </table>
            ) : (
                <div>Loading users...</div>
            )}

            {addModal}
            {editModal}
            {passwordModal}
        </div>
    );
}

export default User;
