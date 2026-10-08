import { useState, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { getApplicationGroups, createApplicationGroup, updateApplicationGroup, deleteApplicationGroup } from '../api/application-group-api';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEdit, faTrash } from '@fortawesome/free-solid-svg-icons';
import '../App.css';

function ApplicationGroup() {
    const [groups, setGroups] = useState([]);
    const [showModal, setShowModal] = useState(false);
    const [selectedGroupId, setSelectedGroupId] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [message, setMessage] = useState('');
    const [messageColor, setMessageColor] = useState('');

    const [searchTerm, setSearchTerm] = useState('');
    const [searchQuery, setSearchQuery] = useState('');

    const initialForm = { Application_Group_Name: '', Description: '' };
    const [formData, setFormData] = useState(initialForm);

    const fetchGroups = async () => {
        try {
            const res = await getApplicationGroups(searchQuery);
            setGroups(res.Data || []);
        } catch (err) { console.error(err); }
    };

    const [sortConfig, setSortConfig] = useState({ key: 'Application_Group_Name', direction: 'asc' });

    const sortedGroups = useMemo(() => {
        if (!groups || !Array.isArray(groups)) return [];

        let sortableItems = [...groups];
        if (sortConfig.key !== null) {
            sortableItems.sort((a, b) => {
                let aValue = a[sortConfig.key];
                let bValue = b[sortConfig.key];

                if (aValue === null || aValue === undefined) aValue = '';
                if (bValue === null || bValue === undefined) bValue = '';

                if (sortConfig.key.includes('Date')) {
                    const dateA = new Date(aValue);
                    const dateB = new Date(bValue);
                    return sortConfig.direction === 'asc' ? dateA - dateB : dateB - dateA;
                }

                const comparison = aValue.toString().localeCompare(bValue.toString(), undefined, {
                    sensitivity: 'base',
                    numeric: true
                });

                return sortConfig.direction === 'asc' ? comparison : -comparison;
            });
        }
        return sortableItems;
    }, [groups, sortConfig]);

    const requestSort = (key) => {
        let direction = 'asc';
        if (sortConfig.key === key && sortConfig.direction === 'asc') {
            direction = 'desc';
        }
        setSortConfig({ key, direction });
    };

    useEffect(() => {
        fetchGroups();
    }, [searchQuery]);

    const handleSearch = () => {
        setSearchQuery(searchTerm);
    };

    const openAddModal = () => {
        setSelectedGroupId(null);
        setFormData(initialForm);
        setMessage('');
        setShowModal(true);
    };

    const openEditModal = (group) => {
        setSelectedGroupId(group.Id);
        setFormData({
            Id: group.Id,
            Application_Group_Name: group.Application_Group_Name,
            Description: group.Description || ''
        });
        setMessage('');
        setShowModal(true);
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setIsSubmitting(true);
        try {
            const isUpdate = !!selectedGroupId;
            const res = isUpdate ? await updateApplicationGroup(formData) : await createApplicationGroup(formData);
            if (res && res.Success !== false) {
                setMessage(`Application group ${isUpdate ? 'updated' : 'created'}!`);
                setMessageColor('green');
                await fetchGroups();
                setTimeout(() => { setShowModal(false); setMessage(''); }, 1500);
            }
        } catch (err) {
            setMessage(`Error: ${err.message}`);
            setMessageColor('red');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (group) => {
        if (!window.confirm(`Are you sure you want to delete the application group: ${group.Application_Group_Name}?\n\n` +
            `Applications in this group will be hidden from the Applications list unless "Include Inactive Applications Group" is checked.`)) return;
        try {
            const res = await deleteApplicationGroup(group.Id);
            if (res.Success !== false) {
                alert("Application group deleted successfully!");
                await fetchGroups();
            }
        } catch (err) {
            alert("Delete failed: " + err.message);
        }
    };

    const modalPortal = showModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content">
                <h2>{selectedGroupId ? 'Edit Application Group' : 'Add New Application Group'}</h2>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Application Group Name *</label>
                        <input type="text" name="Application_Group_Name" maxLength={128} value={formData.Application_Group_Name} onChange={handleChange} required />
                    </div>
                    <div className="form-group">
                        <label>Description</label>
                        <textarea name="Description" maxLength={256} value={formData.Description} onChange={handleChange} />
                    </div>
                    <div style={{ marginTop: '1rem' }}>
                        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving...' : 'Save Application Group'}</button>
                        <button type="button" onClick={() => setShowModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                    </div>
                    {message && <p style={{ color: messageColor, marginTop: '1rem', fontWeight: 'bold', textAlign: 'center' }}>{message}</p>}
                </form>
            </div>
        </div>, document.body
    ) : null;

    return (
        <div style={{ padding: '20px 0px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }} >
                <h2 style={{ margin: 0 }}>Application Groups</h2>
                <div style={{ display: 'flex', gap: '5px', marginLeft: '10px' }}>
                    <input
                        type="text"
                        placeholder="Search by Group Name or Description..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
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
                <button onClick={openAddModal}>Add Application Group</button>
            </div>

            <table>
                <thead>
                    <tr>
                        <th onClick={() => requestSort('Application_Group_Name')} style={{ cursor: 'pointer' }}>
                            Application Group Name
                        </th>
                        <th onClick={() => requestSort('Description')} style={{ cursor: 'pointer' }}>
                            Description
                        </th>
                        <th onClick={() => requestSort('Created_Date')} style={{ cursor: 'pointer' }}>
                            Created Date
                        </th>
                        <th style={{ textAlign: 'center' }}>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {sortedGroups.map(group => (
                        <tr key={group.Id}>
                            <td>{group.Application_Group_Name}</td>
                            <td>{group.Description}</td>
                            <td>{group.Created_Date ? new Date(group.Created_Date).toLocaleDateString() : ''}</td>
                            <td style={{ textAlign: 'center', minWidth: '100px' }}>
                                <FontAwesomeIcon icon={faEdit} style={{ cursor: 'pointer', marginRight: '12px', color: '#3498db' }} title="Edit" onClick={() => openEditModal(group)} />
                                <FontAwesomeIcon icon={faTrash} style={{ color: '#e74c3c', cursor: 'pointer' }} title="Delete" onClick={() => handleDelete(group)} />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
            {modalPortal}
        </div>
    );
}

export default ApplicationGroup;
