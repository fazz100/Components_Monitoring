import { useState, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { 
    getApplications, 
    saveApplication, 
    deleteApplication, 
    saveDatabase, 
    updateDatabase,
    deleteDatabase, 
    testDbConnection 
} from '../api/application-api'; 
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEdit, faTrash, faBellSlash, faBell, faSave } from '@fortawesome/free-solid-svg-icons';
import { createException, getExceptions, deleteException } from '../api/application-exception-api';
import '../App.css'; 
import { getConfig } from '../config/config';

function ApplicationList({ type = '' }) {
    const [apps, setApps] = useState([]);
    const [allExceptions, setAllExceptions] = useState([]); 
    const [showModal, setShowModal] = useState(false); 
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedAppId, setSelectedAppId] = useState(null); 

    const [message, setMessage] = useState('');
    const [messageColor, setMessageColor] = useState('');

    const [silenceModal, setSilenceModal] = useState(false);
    const [selectedApp, setSelectedApp] = useState(null);
    const [silenceReason, setSilenceReason] = useState('');

    const [viewModal, setViewModal] = useState(false);
    const [selectedViewApp, setSelectedViewApp] = useState(null);
    const [testResults, setTestResults] = useState({}); 

    const [databases, setDatabases] = useState([]); 

    const [searchTerm, setSearchTerm] = useState('');     
    const [searchAppType, setSearchAppType] = useState(''); 
    const [searchQuery, setSearchQuery] = useState('');    
    const [searchAppTypeQuery, setSearchAppTypeQuery] = useState(''); 
    
    
    // non async config fetching
    const [config, setConfig] = useState(null);
    const [isConfigLoading, setIsConfigLoading] = useState(true);

    
    useEffect(() => {
        const fetchConfigData = async () => {
            try {
                const data = await getConfig();
                setConfig(data); // Save it to state
            } catch (error) {
                console.error("Failed to load configuration:", error);
            } finally {
                setIsConfigLoading(false); // Stop showing loading state
            }
        };

        fetchConfigData();
    }, []); // Empty array means this runs exactly once when the component mounts


    
    // This is your "Enum" for logic
    const STATUS = {
        NOT_WORKING: 0,
        WORKING: 1,
        KNOWN_ISSUE: 2,
        NO_LONGER_NEEDED: 3
    };

    const statusMap = {
        0: "Not Working",
        1: "Working",
        2: "Known Issue",
        3: "No Longer Needed"
    };

    const dbBtnStyle = { 
        width: '40px', 
        height: '38px', 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center', 
        padding: '0', 
        flexShrink: '0' 
    };
    
    const initialForm = {
        Application_Name: '',
        Description: '',
        Application_Type: '',
        IP_Address: '',
        URL_Or_App_Name: '',
        Is_Enabled: null,
        Service_Status: null,
        Working_Status: STATUS.WORKING
    };
    const [formData, setFormData] = useState(initialForm);


    const fetchApps = async () => {
        try {
            const [appRes, exRes] = await Promise.all([
                getApplications({type:searchAppType, appName:searchQuery}), // Use searchQuery here
                getExceptions()
            ]);

            // ... existing merge logic ...
            const exceptionList = exRes.Data || [];
            setAllExceptions(exceptionList);
            const mergedApps = (appRes.Data || []).map(app => ({
                ...app,
                IsSilenced: exceptionList.some(ex => ex.Application_Id === app.Id)
            }));
            setApps(mergedApps);

        } catch (err) { console.error(err); }
    };

    // 1. New state for sorting
    const [sortConfig, setSortConfig] = useState({ key: 'Application_Name', direction: 'asc' });

    // 2. Sorting Logic
    const sortedApps = useMemo(() => {
        if (!apps || !Array.isArray(apps)) return [];

        let sortableItems = [...apps];
        if (sortConfig.key !== null) {
            sortableItems.sort((a, b) => {
                let aValue = a[sortConfig.key];
                let bValue = b[sortConfig.key];

                // --- CUSTOM LOGIC FOR STATUS ---
                if (sortConfig.key === 'Working_Status') {
                    // Map the numbers to their strings so we compare "Known Issue" vs "Working"
                    aValue = statusMap[aValue] || "Unknown";
                    bValue = statusMap[bValue] || "Unknown";
                }
                // -------------------------------

                // Standard comparison logic (with null safety)
                const finalA = aValue?.toString().toLowerCase() || '';
                const finalB = bValue?.toString().toLowerCase() || '';

                if (finalA < finalB) {
                    return sortConfig.direction === 'asc' ? -1 : 1;
                }
                if (finalA > finalB) {
                    return sortConfig.direction === 'asc' ? 1 : -1;
                }
                return 0;
            });
        }
        return sortableItems;
    }, [apps, sortConfig]);

    // 3. Request Sort Function
    const requestSort = (key) => {
        let direction = 'asc';
        if (sortConfig.key === key && sortConfig.direction === 'asc') {
            direction = 'desc';
        }
        setSortConfig({ key, direction });
    };

    // Helper to show sort arrows
    const getSortIcon = (name) => {
        if (sortConfig.key !== name) return '↕️';
        return sortConfig.direction === 'asc' ? '🔼' : '🔽';
    };

    // Now this only fires when 'type' changes or the Search button is clicked
    useEffect(() => { 
        fetchApps(); 
    }, [searchAppTypeQuery, searchQuery]);

    const handleSearch = () => {
        setSearchQuery(searchTerm); // This triggers the useEffect above
        setSearchAppTypeQuery(searchAppType);
    };

    const openAddModal = () => {
        setSelectedAppId(null);
        setFormData(initialForm);
        setDatabases([]); 
        setMessage('');
        setShowModal(true);
    };

    const openEditModal = (app) => {
        setSelectedAppId(app.Id);
        setFormData({
            Id: app.Id,
            Application_Name: app.Application_Name,
            Description: app.Description,
            Application_Type: app.Application_Type,
            IP_Address: app.IP_Address,
            URL_Or_App_Name: app.URL_Or_App_Name,
            Is_Enabled: app.Is_Enabled,
            Service_Status: app.Service_Status,
            Working_Status: app.Working_Status 
        });
        setDatabases(app.Databases || []); 
        setMessage('');
        setShowModal(true);
    };

    const openViewModal = (app) => {
        setSelectedViewApp(app);
        setTestResults({}); 
        setViewModal(true);
    };

    const handleTestConnection = async (db) => {
        setTestResults(prev => ({ ...prev, [db.Id]: 'Testing...' }));
        try {
            const res = await testDbConnection(db.Connection_String);
            if (res && res.Data === true) {
                setTestResults(prev => ({ ...prev, [db.Id]: 'Connected' }));
            } else {
                setTestResults(prev => ({ ...prev, [db.Id]: 'Unable to connect' }));
            }
        } catch (err) {
            setTestResults(prev => ({ ...prev, [db.Id]: 'Unable to connect' }));
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this application?")) return;
        try {
            const res = await deleteApplication(id);
            if (res.Success !== false) {
                alert("Application deleted successfully!");
                await fetchApps();
            }
        } catch (err) {
            alert("Delete failed: " + err.message);
        }
    };

    // --- Database Logic ---

    const handleDbFieldChange = (index, field, value) => {
        const updatedDbs = [...databases];
        updatedDbs[index][field] = value;
        setDatabases(updatedDbs);
    };

    const handleUpdateDatabase = async (db) => {
        if (!window.confirm(`Are you sure you want to update the database connection: ${db.App_Database_Name}?`)) return;
        
        setMessage('');
        try {
            const res = await updateDatabase(db);
            if (res === true || res.Success !== false) {
                setMessage('Database updated successfully!');
                setMessageColor('green');
                await fetchApps();
            } else {
                setMessage('Failed to update database.');
                setMessageColor('red');
            }
        } catch (err) {
            setMessage("Error: " + err.message);
            setMessageColor('red');
        }
    };

    const handleAddDatabase = async () => {
        setMessage('');
        const nameInput = document.getElementById('newDbName');
        const descInput = document.getElementById('newDbDesc');
        const connInput = document.getElementById('newDbConn');
        
        const name = nameInput.value;
        const desc = descInput.value;
        const conn = connInput.value;

        if (!name || !conn) {
            alert("Please provide at least a name and connection string.");
            return;
        }

        const newDb = {
            App_Database_Name: name,
            Description: desc,
            Connection_String: conn,
            Application_Id: selectedAppId || null,
            Created_By: 'ADMIN'
        };

        if (selectedAppId) {
            try {
                const res = await saveDatabase(newDb);
                if (res) {
                    setMessage('Database added successfully!');
                    setMessageColor('green');
                    nameInput.value = '';
                    descInput.value = '';
                    connInput.value = '';
                    await fetchApps();
                    // Re-sync local databases state after refresh
                    const updatedApp = (await getApplications({type:type})).Data.find(a => a.Id === selectedAppId);
                    setDatabases(updatedApp.Databases || []);
                }
            } catch (err) { 
                setMessage("Error: " + err.message); 
                setMessageColor('red');
            }
        } else {
            setDatabases([...databases, newDb]);
            nameInput.value = '';
            descInput.value = '';
            connInput.value = '';
        }
    };

    const handleRemoveDatabase = async (db, index) => {
        setMessage('');
        if (selectedAppId && db.Id) {
            if (!window.confirm("Permanently delete this database connection?")) return;
            try {
                const res = await deleteDatabase(db.Id);
                if (res.Success !== false) {
                    setDatabases(databases.filter((_, i) => i !== index));
                    setMessage('Database removed successfully!');
                    setMessageColor('green');
                    await fetchApps();
                }
            } catch (err) { 
                setMessage("Error: " + err.message); 
                setMessageColor('red');
            }
        } else {
            setDatabases(databases.filter((_, i) => i !== index));
        }
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => {
            let nextState = { ...prev, [name]: value };
            if (name === "Application_Type") {
                nextState.Is_Enabled = null;
                nextState.Service_Status = null;
            }
            if (name === "Is_Enabled") {
                nextState.Is_Enabled = value === "true" ? true : value === "false" ? false : null;
            }
            if (name === "Working_Status") {
                nextState.Working_Status = value === "" ? null : parseInt(value, 10);
            }
            return nextState;
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setIsSubmitting(true);
        try {
            const isUpdate = !!selectedAppId;
            const payload = { ...formData, Databases: isUpdate ? [] : databases };
            const res = await saveApplication(payload, isUpdate);
            if (res && res.Success !== false) {
                setMessage(`Application ${isUpdate ? 'updated' : 'created'}!`);
                setMessageColor('green');
                await fetchApps();
                setTimeout(() => { setShowModal(false); setMessage(''); }, 1500);
            }
        } catch (err) {
            setMessage(`Error: ${err.message}`);
            setMessageColor('red');
        } finally {
            setIsSubmitting(false);
        }
    };

    const openSilenceModal = (app) => {
        setSelectedApp(app);
        setSilenceReason('');
        setSilenceModal(true);
    };

    const handleQuickSilence = async () => {
        const payload = { 
            Application_Id: selectedApp.Id, 
            Reason_For_Exception: silenceReason, 
            Created_By: 'ADMIN' 
        };
        try {
            const res = await createException(payload);
            if (res) {
                alert("Exception created: Alerts temporarily silenced for " + config.DAYS_SILENCE_DURATION + " days.");
                setSilenceModal(false);
                fetchApps();
            }
        } catch (err) { alert("Error: " + err.message); }
    };

    const handleRemoveSilence = async (appId) => {
        if (window.confirm("Resume monitoring for this application?")) {
            try {
                const exception = allExceptions.find(ex => ex.Application_Id === appId);
                if (exception) {
                    const res = await deleteException(exception.Id);
                    if (res) {
                        alert("Exception removed: Monitoring resumed.");
                        fetchApps();
                    }
                }
            } catch (err) {
                alert("Failed to resume monitoring: " + err.message);
            }
        }
    };

    // --- Modal Portals ---

    const mainModalPortal = showModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content" style={{ maxWidth: '700px' }}>
                <h2>{selectedAppId ? 'Edit Application' : 'Add New Application'}</h2>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Application Name *</label>
                        <input type="text" name="Application_Name" value={formData.Application_Name} onChange={handleChange} required />
                    </div>
                    <div className="form-group">
                        <label>Description *</label>
                        <textarea name="Description" value={formData.Description} onChange={handleChange} required />
                    </div>
                    <div className="form-group">
                        <label>Application Type *</label>
                        <select name="Application_Type" value={formData.Application_Type} onChange={handleChange} required>
                            <option value="">-- Select Type --</option>
                            <option value="scheduled task">Scheduled Task</option>
                            <option value="windows service">Windows Service</option>
                            <option value="website">Website</option>
                        </select>
                    </div>
                    <div className="form-group">
                        <label>Server Name / IP Address *</label>
                        <input type="text" name="IP_Address" value={formData.IP_Address} onChange={handleChange} required />
                    </div>
                    <div className="form-group">
                        <label>URL or App Name *</label>
                        <input type="text" name="URL_Or_App_Name" value={formData.URL_Or_App_Name} onChange={handleChange} required />
                    </div>
                    
                    <div className="form-group">
                        <label>Status *</label>
                        <select 
                            name="Working_Status" 
                            value={formData.Working_Status ?? ""} 
                            onChange={handleChange} 
                            required
                        >
                            <option value="" disabled>Select Status</option>
                            <option value={STATUS.WORKING}>Working</option>
                            <option value={STATUS.NOT_WORKING}>Not Working</option>
                            <option value={STATUS.KNOWN_ISSUE}>Known Issue</option>
                            <option value={STATUS.NO_LONGER_NEEDED}>No Longer Needed</option>
                        </select>
                    </div>

                    <div className="form-group">
                        <label>Is Enabled (Scheduled Task Only)</label>
                        <select 
                            name="Is_Enabled" 
                            required={formData.Application_Type === 'scheduled task'}
                            disabled={formData.Application_Type !== 'scheduled task'}
                            value={formData.Is_Enabled === null ? "" : formData.Is_Enabled.toString()} 
                            onChange={handleChange}
                        >
                            <option value="">-- N/A --</option>
                            <option value="true">Yes</option>
                            <option value="false">No</option>
                        </select>
                    </div>
                    <div className="form-group">
                        <label>Service Status (Windows Service Only)</label>
                        <select 
                            name="Service_Status" 
                            required={formData.Application_Type === 'windows service'}
                            disabled={formData.Application_Type !== 'windows service'}
                            value={formData.Service_Status || ""} 
                            onChange={handleChange}
                        >
                            <option value="">-- N/A --</option>
                            <option value="Running">Running</option>
                            <option value="Stopped">Stopped</option>
                        </select>
                    </div>

                    <div className="database-section" style={{ borderTop: '1px solid #ccc', marginTop: '1rem', paddingTop: '1rem' }}>
                        <h4>Manage Database Connections</h4>
                        {databases.map((db, index) => (
                            <div key={index} className="database-edit-row" style={{ display: 'flex', flexDirection: 'column', gap: '5px', marginBottom: '15px', padding: '10px', backgroundColor: '#f9f9f9', borderRadius: '4px' }}>
                                <div style={{ display: 'flex', gap: '5px' }}>
                                    <input type="text" placeholder="DB Name" value={db.App_Database_Name || ''} onChange={(e) => handleDbFieldChange(index, 'App_Database_Name', e.target.value)} style={{ flex: 1 }} />
                                    <input type="text" placeholder="Description" value={db.Description || ''} onChange={(e) => handleDbFieldChange(index, 'Description', e.target.value)} style={{ flex: 1 }} />
                                </div>
                                <div style={{ display: 'flex', gap: '5px' }}>
                                    <input type="text" placeholder="Connection String" value={db.Connection_String || ''} onChange={(e) => handleDbFieldChange(index, 'Connection_String', e.target.value)} style={{ flex: 2 }} />
                                    {db.Id && (
                                        <button type="button" title="Update Database" onClick={() => handleUpdateDatabase(db)} 
                                            style={{ ...dbBtnStyle, backgroundColor: '#28a745', color: 'white' }}>
                                            <FontAwesomeIcon icon={faSave} />
                                        </button>
                                    )}
                                    <button type="button" title="Delete Database" onClick={() => handleRemoveDatabase(db, index)} 
                                        style={{ ...dbBtnStyle, backgroundColor: '#d9534f', color: 'white' }}>X</button>
                                </div>
                            </div>
                        ))}

                        <div className="database-add-row" style={{ marginTop: '10px', padding: '10px', border: '1px dashed #bbb' }}>
                            <p style={{ margin: '0 0 5px 0', fontSize: '0.85rem', fontWeight: 'bold' }}>Add New Connection:</p>
                            <div style={{ display: 'flex', gap: '5px', marginBottom: '5px' }}>
                                <input type="text" id="newDbName" placeholder="Name" style={{ flex: 1 }} />
                                <input type="text" id="newDbDesc" placeholder="Description" style={{ flex: 1 }} />
                            </div>
                            <div style={{ display: 'flex', gap: '5px' }}>
                                <input type="text" id="newDbConn" placeholder="Connection String" style={{ flex: 2 }} />
                                <button type="button" onClick={handleAddDatabase} 
                                    style={{ ...dbBtnStyle, backgroundColor: '#007bff', color: 'white' }}>+</button>
                            </div>
                        </div>
                    </div>

                    {/* <div className="database-section" style={{ borderTop: '1px solid #ccc', marginTop: '1rem', paddingTop: '1rem' }}>
                        <h4>Manage Database Connections</h4>
                        {databases.map((db, index) => (
                            <div key={index} className="database-edit-row" style={{ display: 'flex', flexDirection: 'column', gap: '5px', marginBottom: '15px', padding: '10px', backgroundColor: '#f9f9f9', borderRadius: '4px' }}>
                                <div style={{ display: 'flex', gap: '5px' }}>
                                    <input type="text" placeholder="DB Name" value={db.App_Database_Name || ''} onChange={(e) => handleDbFieldChange(index, 'App_Database_Name', e.target.value)} style={{ flex: 1 }} />
                                    <input type="text" placeholder="Description" value={db.Description || ''} onChange={(e) => handleDbFieldChange(index, 'Description', e.target.value)} style={{ flex: 1 }} />
                                </div>
                                <div style={{ display: 'flex', gap: '5px' }}>
                                    <input type="text" placeholder="Connection String" value={db.Connection_String || ''} onChange={(e) => handleDbFieldChange(index, 'Connection_String', e.target.value)} style={{ flex: 2 }} />
                                    {db.Id && (
                                        <button type="button" title="Update Database" onClick={() => handleUpdateDatabase(db)} style={{ backgroundColor: '#28a745', color: 'white' }}>
                                            <FontAwesomeIcon icon={faSave} />
                                        </button>
                                    )}
                                    <button type="button" title="Delete Database" onClick={() => handleRemoveDatabase(db, index)} style={{ backgroundColor: '#d9534f', padding: '2px 10px', color: 'white' }}>X</button>
                                </div>
                            </div>
                        ))}
                        
                        <div className="database-add-row" style={{ marginTop: '10px', padding: '10px', border: '1px dashed #bbb' }}>
                            <p style={{ margin: '0 0 5px 0', fontSize: '0.85rem', fontWeight: 'bold' }}>Add New Connection:</p>
                            <div style={{ display: 'flex', gap: '5px', marginBottom: '5px' }}>
                                <input type="text" id="newDbName" placeholder="Name" style={{ flex: 1 }} />
                                <input type="text" id="newDbDesc" placeholder="Description" style={{ flex: 1 }} />
                            </div>
                            <div style={{ display: 'flex', gap: '5px' }}>
                                <input type="text" id="newDbConn" placeholder="Connection String" style={{ flex: 2 }} />
                                <button type="button" onClick={handleAddDatabase}>+</button>
                            </div>
                        </div>
                    </div> */}

                    <div style={{ marginTop: '1rem' }}>
                        <button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving...' : 'Save Application'}</button>
                        <button type="button" onClick={() => setShowModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                    </div>
                    {message && <p style={{ color: messageColor, marginTop: '1rem', fontWeight: 'bold', textAlign: 'center' }}>{message}</p>}
                </form>
            </div>
        </div>, document.body
    ) : null;

    const silenceModalPortal = silenceModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content">
                <h3>Silence Alerts: {selectedApp?.Application_Name}</h3>
                <p>Reason for silencing alerts:</p>
                <textarea 
                    className="form-control"
                    style={{ width: '100%', minHeight: '100px' }}
                    value={silenceReason}
                    onChange={(e) => setSilenceReason(e.target.value)}
                    placeholder="Maintenance, known issue, etc."
                />
                <div style={{ marginTop: '1rem', display: 'flex', gap: '10px' }}>
                    <button onClick={handleQuickSilence}>Confirm</button>
                    <button onClick={() => setSilenceModal(false)} style={{ backgroundColor: '#555' }}>Cancel</button>
                </div>
            </div>
        </div>, document.body
    ) : null;

    const viewModalPortal = viewModal ? createPortal(
        <div className="modal-overlay">
            <div className="modal-content" style={{ maxWidth: '600px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h2>Application Details</h2>
                    <button onClick={() => setViewModal(false)} style={{ backgroundColor: '#555' }}>Close</button>
                </div>
                <hr />
                <div className="read-only-form">
                    <div className="form-group"><label>Name</label><input type="text" value={selectedViewApp?.Application_Name || ''} readOnly /></div>
                    <div className="form-group"><label>Description</label><textarea value={selectedViewApp?.Description || ''} readOnly /></div>
                    <div className="form-group"><label>Type</label><input type="text" value={selectedViewApp?.Application_Type || ''} readOnly /></div>
                    <div className="form-group"><label>Server Name / IP</label><input type="text" value={selectedViewApp?.IP_Address || ''} readOnly /></div>
                    <div className="form-group"><label>Status</label><input type="text" value={statusMap[selectedViewApp?.Working_Status] || ''} readOnly /></div>
                </div>
                
                <div className="database-section" style={{ borderTop: '1px solid #ccc', marginTop: '1rem', paddingTop: '1rem' }}>
                    <h4>Database Connections</h4>
                    {selectedViewApp?.Databases?.length > 0 ? (
                        selectedViewApp.Databases.map((db) => (
                            <div 
                                key={db.Id} 
                                style={{ 
                                    backgroundColor: '#f4f4f4', 
                                    padding: '15px', 
                                    borderRadius: '5px', 
                                    marginBottom: '15px', 
                                    border: '1px solid #ddd',
                                    /* FIX: Prevents the border from stretching to the full modal width */
                                    maxWidth: '500px', 
                                    marginLeft: 'auto',
                                    marginRight: 'auto' 
                                }}
                            >
                                {/* Name Field */}
                                <div className="form-group" style={{ marginBottom: '10px' }}>
                                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Name</label>
                                    <input 
                                        type="text" 
                                        value={db.App_Database_Name} 
                                        readOnly 
                                        style={{ 
                                            width: '100%', 
                                            padding: '8px', 
                                            boxSizing: 'border-box', 
                                            border: '1px solid #ccc',
                                            borderRadius: '4px'
                                        }} 
                                    />
                                </div>

                                {/* Action Row */}
                                <div style={{ marginTop: '10px' }}>
                                    <button 
                                        onClick={() => handleTestConnection(db)} 
                                        style={{ 
                                            padding: '6px 12px', 
                                            fontSize: '0.85rem', 
                                            cursor: 'pointer',
                                            backgroundColor: '#007bff',
                                            color: 'white',
                                            border: 'none',
                                            borderRadius: '4px'
                                        }}
                                    >
                                        Test Connection
                                    </button>
                                    
                                    {testResults[db.Id] && (
                                        <div style={{ 
                                            marginTop: '8px', 
                                            fontSize: '0.9rem',
                                            fontWeight: 'bold', 
                                            color: testResults[db.Id] === 'Connected' ? '#28a745' : '#d9534f' 
                                        }}>
                                            Status: {testResults[db.Id]}
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))
                        // selectedViewApp.Databases.map((db) => (
                        //     <div key={db.Id} style={{ backgroundColor: '#f4f4f4', padding: '10px', borderRadius: '5px', marginBottom: '10px' }}>
                        //         <div className="form-group"><label>Name</label><input type="text" value={db.App_Database_Name} readOnly /></div>
                        //         <button onClick={() => handleTestConnection(db)} style={{ padding: '2px 8px', fontSize: '0.8rem' }}>Test Connection</button>
                                
                        //         {testResults[db.Id] && (
                        //             <div style={{ marginTop: '5px', fontWeight: 'bold', color: testResults[db.Id] === 'Connected' ? 'green' : 'red' }}>
                        //                 Result: {testResults[db.Id]}
                        //             </div>
                        //         )}
                        //     </div>
                            
                        // ))
                    ) : <p>No databases assigned.</p>}
                </div>
            </div>
        </div>, document.body
    ) : null;


    return (
        <div style={{ padding: '20px 0px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }} >
                <h2 style={{ margin: 0 }}>{type} Applications</h2>
                <div style={{ display: 'flex', gap: '5px', marginLeft: '10px' }}>
                    <input 
                        type="text" 
                        placeholder="Search by Application Name, Description, Server Name / IP Address..." 
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
                    
                    <label style={{ 
                            padding: '8px 12px'
                        }}>Application Type</label>
                        <select name="Application_Type2" value={searchAppType} onChange={(e) => setSearchAppType(e.target.value)}>
                            <option value="">-- Select Type --</option>
                            <option value="scheduled task">Scheduled Task</option>
                            <option value="windows service">Windows Service</option>
                            <option value="website">Website</option>
                    </select>
                    <button 
                        type="button" 
                        onClick={handleSearch}
                        style={{ padding: '8px 15px' }}
                    >
                        Search
                    </button>
                    <button 
                        type="button" 
                        onClick={() => { setSearchTerm(''); setSearchQuery(''); setSearchAppType(''); setSearchAppTypeQuery(''); }}
                        style={{ padding: '8px 15px', backgroundColor: '#eee', color: '#555' }}
                    >
                        Clear
                    </button>
                </div>
                <button onClick={openAddModal}>Add Application</button>
            </div>
            
            {apps ? (
                <table>
                    <thead>
                        <tr>
                            {/* Make relevant headers clickable for sorting */}
                            <th onClick={() => requestSort('Application_Name')} style={{ cursor: 'pointer' }}>
                                Application Name
                            </th>
                            <th onClick={() => requestSort('Description')} style={{ cursor: 'pointer' }}>
                                Description
                            </th>
                            <th onClick={() => requestSort('Application_Type')} style={{ cursor: 'pointer' }}>
                                Type
                            </th>
                            <th onClick={() => requestSort('IP_Address')} style={{ cursor: 'pointer' }}>
                                Server Name / IP
                            </th>
                            <th onClick={() => requestSort('URL_Or_App_Name')} style={{ cursor: 'pointer' }}>
                                URL / App Name
                            </th>
                            <th onClick={() => requestSort('Working_Status')} style={{ cursor: 'pointer', textAlign: 'center' }}>
                                Status
                            </th>
                            <th onClick={() => requestSort('Is_Enabled')} style={{ cursor: 'pointer' }}>
                                Enabled
                            </th>
                            <th onClick={() => requestSort('Service_Status')} style={{ cursor: 'pointer' }}>
                                Service Status
                            </th>
                            <th onClick={() => requestSort('Created_Date')} style={{ cursor: 'pointer' }}>
                                Created Date
                            </th>
                            <th style={{ textAlign: 'center' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {/* Changed from 'apps.map' to 'sortedApps.map' */}
                        {sortedApps.map((app) => (
                            <tr key={app.Id}>
                                <td>
                                    <span className="app-link" onClick={() => openViewModal(app)} style={{ color: '#007bff', cursor: 'pointer', textDecoration: 'none', fontWeight: 'bold' }}>
                                        {app.Application_Name}
                                    </span>
                                </td>
                                <td>{app.Description}</td>
                                <td>{app.Application_Type}</td>
                                <td>{app.IP_Address}</td>
                                <td>
                                    {app.Application_Type === "website" ? (
                                        <a href={app.URL_Or_App_Name} target="_blank" rel="noopener noreferrer">{app.URL_Or_App_Name}</a>
                                    ) : app.URL_Or_App_Name}
                                </td>
                                <td style={{ textAlign: 'center' }} title={statusMap[app.Working_Status] || "Unknown"}>
                                    {statusMap[app.Working_Status] || "Unknown"}
                                </td>
                                <td style={{ textAlign: 'center' }}>{app.Is_Enabled === null ? "" : (app.Is_Enabled ? "Yes" : "No")}</td>
                                <td style={{ textAlign: 'left' }}>{app.Service_Status}</td>
                                <td>{app.Created_Date ? new Date(app.Created_Date).toLocaleDateString() : ''}</td>
                                <td style={{ textAlign: 'center', minWidth:'100px' }}>
                                    <FontAwesomeIcon icon={faEdit} style={{ cursor: 'pointer', marginRight: '12px', color: '#3498db' }} title="Edit" onClick={() => openEditModal(app)} />
                                    <FontAwesomeIcon icon={faTrash} style={{ color: '#e74c3c', cursor: 'pointer', marginRight: '12px' }} title="Delete" onClick={() => handleDelete(app.Id)} />
                                    <FontAwesomeIcon 
                                        icon={app.IsSilenced ? faBellSlash : faBell} 
                                        title={app.IsSilenced ? "Resume Monitoring" : "Silence Alerts"}
                                        style={{ color: app.IsSilenced ? '#f39c12' : '#2ecc71', cursor: 'pointer' }} 
                                        onClick={() => app.IsSilenced ? handleRemoveSilence(app.Id) : openSilenceModal(app)} 
                                    />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            ) : <p>Loading applications...</p>}
            {mainModalPortal}
            {silenceModalPortal}
            {viewModalPortal}
        </div>
    );
}

export default ApplicationList;
