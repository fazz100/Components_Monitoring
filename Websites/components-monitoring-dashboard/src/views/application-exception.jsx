import { useState, useEffect, useMemo } from 'react';
import { getExceptions, deleteException, createException } from '../api/application-exception-api';
import { getApplications } from '../api/application-api';
import { getApplicationGroups } from '../api/application-group-api';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { getConfig } from '../config/config';

function ApplicationException() {
    const [exceptions, setExceptions] = useState([]);
    const [apps, setApps] = useState([]);
    const [groups, setGroups] = useState([]);
    const [showModal, setShowModal] = useState(false);
    const [silenceMode, setSilenceMode] = useState('app'); // 'app' = Single Component Mute, 'group' = Entire System Group Mute
    const [formData, setFormData] = useState({ Application_Id: '', Application_Group_Id: '', Reason_For_Exception: '' });

    const [searchTerm, setSearchTerm] = useState('');     
    const [searchQuery, setSearchQuery] = useState('');   

    // non async config fetching
    const [config, setConfig] = useState(null);
    const [, setIsConfigLoading] = useState(true);

    
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

    const fetchData = async () => {
        try {
            const [exRes, appRes, groupRes] = await Promise.all([
                getExceptions(searchQuery),
                getApplications({includeExceptions:false}),
                getApplicationGroups()
            ]);
            setExceptions(exRes.Data || []);
            setApps(appRes.Data || []);
            setGroups(groupRes.Data || []);
        } catch (err) { console.error(err); }
    };

    // 1. New state for sorting
    const [sortConfig, setSortConfig] = useState({ key: 'Application_Name', direction: 'asc' });

    // 2. Sorting Logic
    const sortedExceptions = useMemo(() => {
        // Safety check to prevent "not iterable" error
        if (!exceptions || !Array.isArray(exceptions)) return [];

        let sortableItems = [...exceptions];
        
        if (sortConfig.key !== null) {
            sortableItems.sort((a, b) => {
                let aValue = a[sortConfig.key];
                let bValue = b[sortConfig.key];

                // 1. Handle Nulls/Undefined
                if (aValue === null || aValue === undefined) aValue = '';
                if (bValue === null || bValue === undefined) bValue = '';

                // 2. Handle Dates (Optional but recommended if sorting by Created_Date)
                // If it's a date column, we can compare them as actual timestamps
                if (sortConfig.key.includes('Date')) {
                    const dateA = new Date(aValue);
                    const dateB = new Date(bValue);
                    return sortConfig.direction === 'asc' ? dateA - dateB : dateB - dateA;
                }

                // 3. Robust String Comparison (Reason_For_Exception, etc.)
                const stringA = aValue.toString();
                const stringB = bValue.toString();

                const comparison = stringA.localeCompare(stringB, undefined, {
                    sensitivity: 'base',
                    numeric: true
                });

                return sortConfig.direction === 'asc' ? comparison : -comparison;
            });
        }
        return sortableItems;
    }, [exceptions, sortConfig]);

    // 3. Request Sort Function
    const requestSort = (key) => {
        let direction = 'asc';
        if (sortConfig.key === key && sortConfig.direction === 'asc') {
            direction = 'desc';
        }
        setSortConfig({ key, direction });
    };
   

    // Now this only fires when 'type' changes or the Search button is clicked
    useEffect(() => { 
        fetchData(); 
    }, [searchQuery]);

    const handleSearch = () => {
        setSearchQuery(searchTerm); // This triggers the useEffect above
    };



    const handleSave = async (e) => {
        e.preventDefault();
        // send only the target of the selected mode; the API requires exactly one of the two
        const payload = {
            Application_Id: silenceMode === 'app' ? formData.Application_Id : null,
            Application_Group_Id: silenceMode === 'group' ? formData.Application_Group_Id : null,
            Reason_For_Exception: formData.Reason_For_Exception
        };
        try {
            const res = await createException(payload);
            if (res.Data === true) {
                alert("Exception created: Alerts temporarily silenced for " + config.DAYS_SILENCE_DURATION + " days.");
                setShowModal(false);
                setSilenceMode('app');
                setFormData({ Application_Id: '', Application_Group_Id: '', Reason_For_Exception: '' });
                fetchData();
            }
        } catch (err) { alert("Error: " + err.message); }
    };

    // groups that are not already silenced
    const availableGroups = groups.filter(g => !exceptions.some(ex => ex.Application_Group_Id === g.Id));

    const handleRemove = async (id) => {
        if (window.confirm("Resume monitoring for this application?")) {
            await deleteException(id);
            fetchData();
        }
    };

    

    return (
        <div style={{ padding: '20px 0px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }} >
                <h2 style={{ margin: 0 }}>Exceptions</h2>
                <div style={{ display: 'flex', gap: '5px', marginLeft: '10px' }}>
                    <input 
                        type="text" 
                        placeholder="Search by Application Name..." 
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
                <button onClick={() => setShowModal(true)}>Add Exception</button>
            </div>

            {/* <table>
                <thead>
                    <tr>
                        <th>Application Name</th>
                        <th>Reason</th>
                        <th>Created Date (UTC)</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {exceptions.map(ex => (
                        <tr key={ex.Id}>
                            <td>{ex.Application_Name || ex.Application_Id}</td>
                            <td>{ex.Reason_For_Exception}</td>
                            <td>{new Date(ex.Created_Date).toLocaleString()}</td>
                            <td style={{ textAlign: 'center' }}>
                                <FontAwesomeIcon icon={faTrash} style={{ color: '#d9534f', cursor: 'pointer', marginRight: '12px' }} onClick={() => handleRemove(ex.Id)} />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table> */}
            <table>
                <thead>
                    <tr>
                        <th onClick={() => requestSort('Application_Name')} style={{ cursor: 'pointer' }}>
                            Application Name
                        </th>
                        <th onClick={() => requestSort('Reason_For_Exception')} style={{ cursor: 'pointer' }}>
                            Reason
                        </th>
                        <th onClick={() => requestSort('Created_Date')} style={{ cursor: 'pointer' }}>
                            Created Date (UTC)
                        </th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {/* 4. Use sortedExceptions here instead of exceptions */}
                    {sortedExceptions.map(ex => (
                        <tr key={ex.Id}>
                            <td>{ex.Application_Group_Id ? `System: ${ex.Group_Name}` : (ex.Application_Name || ex.Application_Id)}</td>
                            <td>{ex.Reason_For_Exception}</td>
                            <td>{new Date(ex.Created_Date).toLocaleString()}</td>
                            <td style={{ textAlign: 'center' }}>
                                <FontAwesomeIcon icon={faTrash} style={{ color: '#d9534f', cursor: 'pointer' }} onClick={() => handleRemove(ex.Id)} />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {showModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <h3>Silence Application</h3>
                        <form onSubmit={handleSave}>
                            <label>Silence:</label>
                            <div style={{ display: 'flex', gap: '20px', marginBottom: '10px' }}>
                                <label style={{ display: 'flex', alignItems: 'center', gap: '5px', fontWeight: 'normal' }}>
                                    <input type="radio" name="silenceMode" value="app" checked={silenceMode === 'app'} onChange={() => setSilenceMode('app')} style={{ width: 'auto' }} />
                                    Single Component
                                </label>
                                <label style={{ display: 'flex', alignItems: 'center', gap: '5px', fontWeight: 'normal' }}>
                                    <input type="radio" name="silenceMode" value="group" checked={silenceMode === 'group'} onChange={() => setSilenceMode('group')} style={{ width: 'auto' }} />
                                    Entire System Group
                                </label>
                            </div>
                            {silenceMode === 'app' ? (
                                <>
                                    <label>Select Application:</label>
                                    <select required value={formData.Application_Id} onChange={e => setFormData({...formData, Application_Id: e.target.value})}>
                                        <option value="">-- Select --</option>
                                        {apps.map(a => <option key={a.Id} value={a.Id}>{a.Application_Name}</option>)}
                                    </select>
                                </>
                            ) : (
                                <>
                                    <label>Select Application Group:</label>
                                    <select required value={formData.Application_Group_Id} onChange={e => setFormData({...formData, Application_Group_Id: e.target.value})}>
                                        <option value="">-- Select --</option>
                                        {availableGroups.map(g => <option key={g.Id} value={g.Id}>{g.Application_Group_Name}</option>)}
                                    </select>
                                </>
                            )}
                            <label>Reason:</label>
                            <textarea required value={formData.Reason_For_Exception} onChange={e => setFormData({...formData, Reason_For_Exception: e.target.value})} />
                            <div style={{ marginTop: '1rem' }}>
                                <button type="submit" >Silence Now</button>
                                <button type="button" onClick={() => setShowModal(false)} style={{ marginLeft: '10px', backgroundColor: '#555' }}>Cancel</button>
                            </div>
                            {/* <button type="submit">Silence Now</button>
                            <button type="button" onClick={() => setShowModal(false)}>Cancel</button> */}
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ApplicationException;