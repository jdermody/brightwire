// import React, { useRef, useCallback } from 'react';

// declare module vis{}

// export interface Point3D {
//     x: number;
//     y: number;
//     z: number;
//     color: string;
// }

// interface VisPoint3D {
//     id: string|number;
//     x: number;
//     y: number;
//     z: number;
//     style?: string|number;
// }

// export interface ScatterPlot3DProps {
//     points: Array<Point3D>;
// }

// interface ColorInfo {
//     fill: string;
//     stroke: string;
//     strokeWidth: number;
// }

// interface CameraPosition {
//     horizontal: number;
//     vertical: number;
//     distance: number;
// }

// interface Graph3dOptions {
//     animationInterval: number;
//     animationPreload: boolean;
//     animationAutoStart: boolean;
//     axisColor: string;
//     backgroundColor: string|ColorInfo;
//     cameraPosition: CameraPosition;
//     dataColor: string|ColorInfo;
//     dotSizeRatio: number;
//     dotSizeMinFraction: number;
//     dotSizeMaxFraction: number;
//     gridColor: string;
//     height: string;
//     keepAspectRatio: boolean;
//     showGrid: boolean;
//     showXAxis: boolean;
//     showYAxis: boolean;
//     showZAxis: boolean;
//     showPerspective: boolean;
//     showLegend: boolean;
//     showShadow: boolean;
//     style: "bar"|"bar-color"|"bar-size"|"dot"|"dot-line"|"dot-color"|"dot-size"|"line"|"grid"|"surface";
//     valueMax: number;
//     valueMin: number;
//     verticalRatio: number;
//     width: string;
//     xLabel: string;
//     yLabel: string;
//     zLabel: string;
//     filterLabel: string;
//     legendLabel: string;
// }

// var options: Partial<Graph3dOptions> = {
//     width:  '500px',
//     height: '500px',
//     style: 'dot-color',
//     showPerspective: true,
//     showGrid: true,
//     keepAspectRatio: true,
//     verticalRatio: 1.0,
//     showLegend: false,
//     backgroundColor: '#fffff'
//     //legendLabel: 'distance',
//     // cameraPosition: {
//     //     horizontal: -0.35,
//     //     vertical: 0.22,
//     //     distance: 1.8
//     // }
// };

// export const ScatterPlot3D = ({points}: ScatterPlot3DProps) => {
//     const graph = useRef<any>(null);
//     const plot = useCallback((el: HTMLDivElement) => {
//         if(el && !graph.current) {
//             const dataSet = new vis.DataSet<VisPoint3D>();
//             let index = 0;
//             for(const row of points) {
//                 dataSet.add({
//                     id: index++,
//                     x: row.x,
//                     y: row.y,
//                     z: row.z,
//                     style: row.color
//                 });
//             }
//             graph.current = new vis.Graph3d(el, dataSet, options);
//         }
//     }, [points]);
//     return <div ref={plot} className="scatterplot-3d" />;
// };