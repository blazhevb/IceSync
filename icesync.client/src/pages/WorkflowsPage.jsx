import { useEffect, useState } from 'react';
import WorkflowsTable from '../components/WorkflowsTable';
import { fetchWorkflows } from '../services/workflowsApi';

const WorkflowsPage = () => {
    const [workflows, setWorkflows] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const getWorkflows = async () => {
            try {
                const data = await fetchWorkflows();
                setWorkflows(data);
            } catch (err) {
                setError('Failed to load workflows');
            } finally {
                setLoading(false);
            }
        };
        getWorkflows();
    }, []);

    if (loading) {
        return <div className="container mx-auto p-4">Loading...</div>;
    }

    if (error) {
        return <div className="container mx-auto p-4">{error}</div>;
    }

    return (
        <div className="container mx-auto p-4 bg-gradient-to-r from-blue-500 to-purple-600 rounded-lg shadow-lg">
            <h1 className="text-3xl font-bold mb-6 text-white">Workflows</h1>
            <div className="bg-white p-6 rounded-lg shadow-lg">
                <WorkflowsTable workflows={workflows} />
            </div>
        </div>
    );
};

export default WorkflowsPage;
