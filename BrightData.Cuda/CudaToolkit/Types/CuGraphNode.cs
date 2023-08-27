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
                var res = DriverApiNativeMethods.GraphManagement.cuGraphNodeGetType(this, ref type);
                if (res != CuResult.Success) throw new CudaException(res);
                return type;
            }
        }
        public void SetParameters(CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphHostNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphKernelNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphMemcpyNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphMemsetNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphExternalSemaphoresSignalNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphExternalSemaphoresWaitNodeSetParams(this, nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void SetParameters(CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphBatchMemOpNodeSetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaHostNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphHostNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaKernelNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphKernelNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemCpy3D nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphMemcpyNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemsetNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphMemsetNodeGetParams(this, ref nodeParams);
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemSignalNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphExternalSemaphoresSignalNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(CudaExtSemWaitNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphExternalSemaphoresWaitNodeGetParams(this, nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaMemAllocNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphMemAllocNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CuDevicePtr nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphMemFreeNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public void GetParameters(ref CudaBatchMemOpNodeParams nodeParams)
        {
            var res = DriverApiNativeMethods.GraphManagement.CuGraphBatchMemOpNodeGetParams(this, ref nodeParams);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuGraphNode[]? GetDependencies()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagement.cuGraphNodeGetDependencies(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CuGraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagement.cuGraphNodeGetDependencies(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public CuGraphNode[]? GetDependentNodes()
        {
            var numNodes = new SizeT();
            var res = DriverApiNativeMethods.GraphManagement.cuGraphNodeGetDependentNodes(this, null, ref numNodes);
            
            if (res != CuResult.Success) throw new CudaException(res);

            if (numNodes > 0) {
                var nodes = new CuGraphNode[numNodes];
                res = DriverApiNativeMethods.GraphManagement.cuGraphNodeGetDependentNodes(this, nodes, ref numNodes);
                
                if (res != CuResult.Success) throw new CudaException(res);

                return nodes;
            }

            return null;
        }
        public void CuGraphKernelNodeCopyAttributes(CuGraphNode dst)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphKernelNodeCopyAttributes(dst, this);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuKernelNodeAttrValue GetAttribute(CuKernelNodeAttrId attr)
        {
            var value = new CuKernelNodeAttrValue();
            var res = DriverApiNativeMethods.GraphManagement.cuGraphKernelNodeGetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return value;
        }
        public void SetAttribute(CuKernelNodeAttrId attr, CuKernelNodeAttrValue value)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphKernelNodeSetAttribute(this, attr, ref value);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuEvent GetRecordEvent()
        {
            var eventOut = new CuEvent();
            var res = DriverApiNativeMethods.GraphManagement.cuGraphEventRecordNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetRecordEvent(CuEvent @event)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphEventRecordNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
        public CuEvent GetWaitEvent()
        {
            var eventOut = new CuEvent();
            var res = DriverApiNativeMethods.GraphManagement.cuGraphEventWaitNodeGetEvent(this, ref eventOut);
            
            if (res != CuResult.Success) throw new CudaException(res);
            return eventOut;
        }
        public void SetWaitEvent(CuEvent @event)
        {
            var res = DriverApiNativeMethods.GraphManagement.cuGraphEventWaitNodeSetEvent(this, @event);
            
            if (res != CuResult.Success) throw new CudaException(res);
        }
    }
}
