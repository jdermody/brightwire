using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuGraphNode
    {
        public nint Pointer;
        public CuGraphNodeType Type
        {
            get
            {
                var type = new CuGraphNodeType();
                var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetType(this, ref type);
                if (res != CuResult.Success) throw new CudaException(res);
                return type;
            }
        }
        public void SetParameters(CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphHostNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemcpyNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemsetNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresSignalNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresWaitNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphBatchMemOpNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphHostNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemcpyNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemsetNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresSignalNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphExternalSemaphoresWaitNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemAllocNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphMemAllocNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CuDevicePtr nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphMemFreeNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagment.CuGraphBatchMemOpNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuGraphNode[]? GetDependencies()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CuGraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependencies(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public CuGraphNode[]? GetDependentNodes()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CuGraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagment.cuGraphNodeGetDependentNodes(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public void CuGraphKernelNodeCopyAttributes(CuGraphNode dst)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeCopyAttributes(dst, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuKernelNodeAttrValue GetAttribute(CuKernelNodeAttrId attr)
        {
            var value = new CuKernelNodeAttrValue();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeGetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return value;
        }
        public void SetAttribute(CuKernelNodeAttrId attr, CuKernelNodeAttrValue value)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphKernelNodeSetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuEvent GetRecordEvent()
        {
            var eventOut = new CuEvent();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventRecordNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetRecordEvent(CuEvent @event)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventRecordNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuEvent GetWaitEvent()
        {
            var eventOut = new CuEvent();
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventWaitNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetWaitEvent(CuEvent @event)
        {
            var res = DriverApiNativeMethods.GraphManagment.cuGraphEventWaitNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
    }
}
