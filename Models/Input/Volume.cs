using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace BrightWire.Models.Input
{
    /// <summary>
    /// A volume is dense layered list of images together with an expected output vector
    /// </summary>
    [ProtoContract]
    public class Volume
    {
        /// <summary>
        /// A single dense image
        /// </summary>
        [ProtoContract]
        public class Layer
        {
            /// <summary>
            /// The image width
            /// </summary>
            [ProtoMember(1)]
            public int Width { get; set; }

            /// <summary>
            /// The image height
            /// </summary>
            [ProtoMember(2)]
            public int Height { get; set; }

            /// <summary>
            /// The image data
            /// </summary>
            [ProtoMember(3)]
            public float[] Data { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public Layer() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="data"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            public Layer(float[] data, int width, int height)
            {
                Data = data;
                Width = width;
                Height = height;
            }

            /// <summary>
            /// Converts the image to a matrix
            /// </summary>
            /// <param name="lap"></param>
            /// <returns></returns>
            public IMatrix AsMatrix(ILinearAlgebraProvider lap)
            {
                return lap.Create(Width, Height, (i, j) => Data[j * Width + i]);
            }
        }

        /// <summary>
        /// The list of images
        /// </summary>
        [ProtoMember(1)]
        public Layer[] Layers { get; set; }

        /// <summary>
        /// The expected output
        /// </summary>
        [ProtoMember(2)]
        public float[] ExpectedOutput { get; set; }

        /// <summary>
        /// Converts the volume to a tensor
        /// </summary>
        /// <param name="lap">The linear algebra provider</param>
        public I3DTensor AsTensor(ILinearAlgebraProvider lap)
        {
            var matrixList = Layers.Select(l => l.AsMatrix(lap)).ToList();
            var ret = lap.CreateTensor(matrixList);
            foreach (var item in matrixList)
                item.Dispose();
            return ret;
        }
    }
}
