import { getConfig } from '../config/config';
import { getAuthToken, handleApiResponse } from '../helpers/auth-token-helper';

async function getHeaders() {
    const config = await getConfig();
    const token = getAuthToken();
    return {
        'Authorization': `Bearer ${token}`,
        'X-Api-Token': config.API_AUTH_TOKEN,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };
}

// --- Application Endpoints ---

// export async function getApplications(type = '') {
//     const config = await getConfig();
//     const url = type 
//         ? `${config.API_BASE_URL}/api/applications/${type}` 
//         : `${config.API_BASE_URL}/api/applications`;
        
//     const response = await fetch(url, {
//         method: 'GET',
//         headers: await getHeaders()
//     });
//     return handleApiResponse(response);
// }

export async function getApplications({ type = null, appName = null, includeExceptions = true } = {}) {
    const config = await getConfig();
    
    // 1. Initialize the base URL
    const baseUrl = `${config.API_BASE_URL}/api/applications`;
    
    // 2. Build the query string dynamically
    const params = new URLSearchParams();
    if (type) params.append('type', type);
    if (appName) params.append('appName', appName);

   // explicit check for true/false since 'false' is a valid value
    if (includeExceptions !== null) params.append('includeExceptions', includeExceptions);

    // 3. Combine them (params.toString() handles the empty case gracefully)
    const queryString = params.toString();
    const finalUrl = queryString ? `${baseUrl}?${queryString}` : baseUrl;

    const response = await fetch(finalUrl, {
        method: 'GET',
        headers: await getHeaders()
    });

    return handleApiResponse(response);
}

export async function saveApplication(model, isUpdate = false) {
    const config = await getConfig();
    const endpoint = isUpdate ? 'update' : 'create';
    const response = await fetch(`${config.API_BASE_URL}/api/applications/${endpoint}`, {
        method: isUpdate ? 'PUT' : 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(model)
    });
    return handleApiResponse(response);
}

export async function deleteApplication(id) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/applications/delete/${id}`, {
        method: 'DELETE',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}

// --- Database Endpoints ---

export async function saveDatabase(dbModel) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-databases/save`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(dbModel)
    });
    return handleApiResponse(response);
}

// NEW: Added updateDatabase to match your Controller
export async function updateDatabase(dbModel) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-databases/update`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(dbModel)
    });
    return handleApiResponse(response);
}

export async function deleteDatabase(id) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-databases/delete/${id}`, {
        method: 'DELETE',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}

export async function testDbConnection(connectionString) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-databases/test-connection`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(connectionString) 
    });
    return handleApiResponse(response);
}
