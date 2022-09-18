// import * as React from 'react';
// import { getColour, getColour2 } from './Colours';
// import { LegendItem } from './Legend';
// import { ColumnInfoModel } from '../models';
// import { getColumnIndex, getName, isClassificationTarget, isNumeric } from '../helper/MetaDataUtil';

// export interface IHaveData
// {
//     data: string[];
// }

// export interface IHaveRowsOfData
// {
//     rows: IHaveData[];
// }

// export interface ScatterPlotProps {
//     data: IHaveRowsOfData;
//     columns: ColumnInfoModel[];
//     baseColumnIndex: number;
//     initialOtherColumnIndex: number;
//     title: string;
//     showSelect: boolean;
//     showTitle: boolean;
//     legendCallback?: (labels: LegendItem[]) => void;
//     clusterColumnIndex?: number;
// }

// interface ScatterPlotState {
//     plot: ChartData;
//     columns: IColumn[];
//     selectedColumnIndex: number;
//     clusterColumnIndex?: number;
// }

// interface IColumn {
//     index: number;
//     title: string;
// }

// interface Point {
//     x: number;
//     y: number;
//     backgroundColor?: string;
// }

// interface Segment {
//     name: string;
//     colour: string;
//     rawData: Point[];
// }

// interface ChartData {
//     segments: Segment[];
//     xMin: number;
//     xRange: number;
//     yMin: number;
//     yRange: number;
// }

// interface DisplayRect {
//     width: number;
//     height: number;
//     paddingX: number;
//     paddingY: number;
// }

// export class ScatterPlot extends React.Component<ScatterPlotProps, ScatterPlotState> {
//     constructor(props: ScatterPlotProps) {
//         super(props);
//         this.state = ScatterPlot.getState(props.initialOtherColumnIndex, props);
//     }

//     static getState(selectedColumnIndex: number, props: ScatterPlotProps): ScatterPlotState {
//         const {baseColumnIndex, data, columns, clusterColumnIndex} = props;
//         return {
//             plot: ScatterPlot.load(baseColumnIndex, selectedColumnIndex, props),
//             columns: columns
//                 .filter(c => !isClassificationTarget(c.metaData) && isNumeric(c.columnType) && getColumnIndex(c.metaData) !== clusterColumnIndex)
//                 .map(c => ({index: getColumnIndex(c.metaData), title: getName(c.metaData)})),
//             selectedColumnIndex
//         };
//     }

//     static getDerivedStateFromProps(props: ScatterPlotProps, state: ScatterPlotState) {
//         if(props.clusterColumnIndex !== state.clusterColumnIndex) {
//             var ret = ScatterPlot.getState(state.selectedColumnIndex, props);
//             ret.clusterColumnIndex = props.clusterColumnIndex;
//             return ret;
//         }
//         return null;
//     }

//     static load(xIndex: number, yIndex: number, props: ScatterPlotProps): ChartData {
//         const {data, clusterColumnIndex, columns} = props;
//         const xValue = (d: IHaveData) => parseFloat(d.data[xIndex]),
//             yValue = (d: IHaveData) => parseFloat(d.data[yIndex]),
//             clusterIndex = typeof(clusterColumnIndex) !== 'undefined' ? (d: IHaveData) => parseInt(d.data[clusterColumnIndex], 10) : null;

//         let targetColumnIndex = columns.filter(c => isClassificationTarget(c.metaData)).map(c => getColumnIndex(c.metaData))[0];
//         if (targetColumnIndex == undefined)
//             targetColumnIndex = columns.length - 1;

//         const classificationMap = new Map<string, Point[]>();
//         const clusterSet = new Set<number>();
//         let xMin = Number.MAX_VALUE, xMax = Number.MIN_VALUE, yMin = Number.MAX_VALUE, yMax = Number.MIN_VALUE;
//         for (let i = 0, len = data.rows.length; i < len; i++) {
//             const row = data.rows[i];
//             const classification = row.data[targetColumnIndex];
//             let array = classificationMap.get(classification);
//             if (!array)
//                 classificationMap.set(classification, array = new Array<Point>());
//             const x = xValue(row);
//             const y = yValue(row);
//             const pt: Point = {x, y};
//             if(clusterIndex) {
//                 const cluster = clusterIndex(row);
//                 clusterSet.add(cluster);
//                 pt.backgroundColor = getColour2(cluster, true)
//             }
//             array.push(pt);
//             if(x > xMax) xMax = x;
//             if (x < xMin) xMin = x;
//             if(y > yMax) yMax = y;
//             if (y < yMin) yMin = y;
//         }
//         let index = 0;
//         const segments: Array<Segment> = [];
//         for (let item of classificationMap.entries()) {
//             const [name, rawData] = item;
//             segments.push({
//                 name,
//                 colour: getColour(index++, true),
//                 rawData
//             });
//         }
//         const {legendCallback} = props;
//         legendCallback && legendCallback(
//             segments.map(s => ({name: s.name, colour: s.colour}))
//                 .concat(Array.from(clusterSet.keys()).sort((c1, c2) => c1 - c2).map(c => ({
//                     name: c.toString(),
//                     colour: getColour2(c, false)
//                 })))
//         );
//         return {
//             segments,
//             xRange: xMax-xMin,
//             xMin,
//             yRange: yMax-yMin,
//             yMin
//         };
//     }

//     drawPoint(point: Point, colour: string, size: number, data: ChartData, context: CanvasRenderingContext2D, display: DisplayRect) {
//         context.beginPath();
//         context.fillStyle = colour;
//         const {width, height, paddingX, paddingY} = display;

//         const px = paddingX + (point.x - data.xMin) / data.xRange * width;
//         const py = paddingY + (point.y - data.yMin) / data.yRange * height;

//         context.arc(px, height - py, size, 0, 2 * Math.PI, true);
//         context.fill();
//     }

//     onLoad(el: HTMLCanvasElement | null) {
//         if (el) {
//             console.log('drawing scatter plot...');
//             const ctx = el.getContext('2d');
//             const parent = el.parentElement;
//             if (parent) {
//                 el.width = parent.offsetWidth;
//                 el.height = parent.offsetWidth;
//             }
//             const {width, height} = el;
//             if (ctx) {
//                 ctx.clearRect(0, 0, width, height);
//                 const {plot} = this.state;
//                 const display: DisplayRect = {
//                     height: height-6,
//                     width: width - 6,
//                     paddingX: 3,
//                     paddingY: 3
//                 };

//                 for (let segment of plot.segments) {
//                     for (let point of segment.rawData) {
//                         if(point.backgroundColor)
//                             this.drawPoint(point, point.backgroundColor, 7, plot, ctx, display);    
//                         this.drawPoint(point, segment.colour, 3, plot, ctx, display);
//                     }
//                 }
//             }
//         }
//     }

//     toggleDisplay(newIndex: number) {
//         this.setState({
//             selectedColumnIndex: newIndex,
//             plot: ScatterPlot.load(this.props.baseColumnIndex, newIndex, this.props)
//         });
//     }

//     render() {
//         const {title, showSelect, showTitle} = this.props;
//         const {selectedColumnIndex, columns} = this.state;
//         return <div className="scatter-plot">
//             {showTitle ? <h1>{title}</h1> : null}
//             <canvas ref={el => this.onLoad(el)}/>
//             {showSelect ? <select className="browser-default" value={selectedColumnIndex} onChange={e => this.toggleDisplay(parseInt(e.currentTarget.value, 10))}>
//                 {columns.map(c => <option key={c.index} value={c.index}>{c.title}</option>)}
//             </select> : null }
//         </div>;
//     }
// }