import React, { useState } from 'react';
import { runWorkflow } from '../services/workflowsApi';

const WorkflowRow = ({ workflow }) => {
    const [status, setStatus] = useState(null);
    const [isRunning, setIsRunning] = useState(false);

    const handleRun = async () => {
        setIsRunning(true);
        const success = await runWorkflow(workflow.workflowID);
        setStatus(success ? 'success' : 'error');
        setIsRunning(false);

        setTimeout(() => setStatus(null), 3000);
    };

    const isButtonDisabled = isRunning || status === 'success' || status === 'error';

    return (
        <tr className="hover:bg-gray-100">
            <td className="py-3 px-4 border-b border-gray-300 text-center">{workflow.workflowID}</td>
            <td className="py-3 px-4 border-b border-gray-300 text-center">{workflow.workflowName}</td>
            <td className="py-3 px-4 border-b border-gray-300 text-center">{workflow.isActive ? 'Yes' : 'No'}</td>
            <td className="py-3 px-4 border-b border-gray-300 text-center">{workflow.multiExecBehavior}</td>
            <td className="py-3 px-4 border-b border-gray-300 text-center">
                <button
                    onClick={handleRun}
                    className={`w-24 py-1 px-3 rounded text-white ${isRunning ? 'bg-gray-400 cursor-not-allowed' :
                            status === 'success' ? 'bg-teal-300' :
                                status === 'error' ? 'bg-red-500' :
                                    'bg-green-500 hover:bg-green-700'
                        }`}
                    disabled={isButtonDisabled}
                >
                    {isRunning ? 'Running...' :
                        status === 'success' ? 'Success' :
                            status === 'error' ? 'Failed' :
                                'Run'}
                </button>
            </td>
        </tr>
    );
};

export default WorkflowRow;
