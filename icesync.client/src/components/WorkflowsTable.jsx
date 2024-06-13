import WorkflowRow from './WorkflowRow';

const WorkflowsTable = ({ workflows }) => {
    return (
        <div className="overflow-x-auto shadow-md rounded-lg">
            <table className="min-w-full bg-white border border-gray-300 rounded-t-lg">
                <thead className="bg-blue-600 text-white">
                    <tr>
                        <th className="py-3 px-4 border-b border-gray-300 text-center">Workflow Id</th>
                        <th className="py-3 px-4 border-b border-gray-300 text-center">Workflow Name</th>
                        <th className="py-3 px-4 border-b border-gray-300 text-center">Is Active</th>
                        <th className="py-3 px-4 border-b border-gray-300 text-center">Multi Exec Behavior</th>
                        <th className="py-3 px-4 border-b border-gray-300 text-center">Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {workflows.length > 0 ? (
                        workflows.map(workflow => (
                            <WorkflowRow key={workflow.workflowId} workflow={workflow} />
                        ))
                    ) : (
                        <tr>
                            <td colSpan="5" className="py-4 px-4 text-center text-gray-500">
                                No workflows available to display.
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
};

export default WorkflowsTable;
