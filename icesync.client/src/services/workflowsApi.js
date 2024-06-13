export const fetchWorkflows = async () => {
    console.log('Fetching workflows...');
    try {
        const response = await fetch('/api/v1/workflow');
        if (response.ok) {
            console.log('Workflows fetched successfully');
            return await response.json();
        } else {
            console.error('Failed to fetch workflows');
            return [];
        }
    } catch (error) {
        console.error('Error fetching workflows:', error);
        return [];
    }
};

export const runWorkflow = async (workflowID) => {
    try {
        const response = await fetch(`/api/v1/workflow/${workflowID}/run`, { method: 'POST' });
        if (response.ok) {
            const responseData = await response.json();
            return responseData.success;
        } else {
            return false;
        }
    } catch (error) {
        console.error('Error:', error);
        return false;
    }
};