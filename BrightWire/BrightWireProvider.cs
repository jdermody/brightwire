using BrightTable;
using BrightTable.Input;
using BrightWire.Bayesian.Training;
using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;

namespace BrightWire
{
    /// <summary>
    /// Main entry point
    /// </summary>
    public static class BrightWireProvider
    {
        /// <summary>
        /// Parses a CSV file into a data table
        /// </summary>
        /// <param name="streamReader">The stream of CSV data</param>
        /// <param name="delimeter">The CSV delimeter</param>
        /// <param name="hasHeader">True if there is a header</param>
        /// <param name="output">A stream to write the data table to (for file based processing) - null for in memory processing</param>
        //public static IDataTable ParseCSV(this StreamReader streamReader, char delimeter = ',', bool? hasHeader = null, Stream output = null)
        //{
        //    var builder = new CSVParser(delimeter);
        //    return builder.Parse(streamReader, output, hasHeader);
        //}

        /// <summary>
        /// Parses a CSV string into a data table
        /// </summary>
        /// <param name="csv">The string to parse</param>
        /// <param name="delimeter">The CSV delimeter</param>
        /// <param name="hasHeader">True if there is a header</param>
        /// <param name="output">A stream to write the data table to (for file based processing) - null for in memory processing</param>
        /// <returns></returns>
        //public static IDataTable ParseCSV(this string csv, char delimeter = ',', bool? hasHeader = null, Stream output = null)
        //{
        //    if (!String.IsNullOrWhiteSpace(csv)) {
        //        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
        //        using (var reader = new StreamReader(stream)) {
        //            return ParseCSV(reader, delimeter, hasHeader, output);
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Parses CSV into a data table without type detection - all columns will be strings
        /// </summary>
        /// <param name="streamReader">The streamn reader that contains the CSV to parse</param>
        /// <param name="delimeter">The CSV delimeter</param>
        /// <param name="hasHeader">True if there is a header</param>
        /// <param name="output">A stream to write the data table to (for file based processing) - null for in memory processing</param>
        /// <returns></returns>
        //public static IRowOrientedDataTable ParseCSVToText(this StreamReader streamReader, char delimeter = ',', bool? hasHeader = null, Stream output = null)
        //{
        //    var parser = new CsvParser(streamReader, delimeter, true);
        //    return builder.Parse(streamReader, output, hasHeader);
        //}

        /// <summary>
        /// Parses CSV into a data table without type detection - all columns will be strings
        /// </summary>
        /// <param name="csv">The string to parse</param>
        /// <param name="delimeter">>The CSV delimeter</param>
        /// <param name="hasHeader">True if there is a header</param>
        /// <param name="output">A stream to write the data table to (for file based processing) - null for in memory processing</param>
        /// <returns></returns>
        //public static IDataTable ParseCSVToText(this string csv, char delimeter = ',', bool? hasHeader = null, Stream output = null)
        //{
        //    if (!String.IsNullOrWhiteSpace(csv)) {
        //        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
        //        using (var reader = new StreamReader(stream)) {
        //            return ParseCSVToText(reader, delimeter, hasHeader, output);
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Creates a data table from a stream
        /// </summary>
        /// <param name="dataStream">The stream that the data table was written to</param>
        /// <param name="indexStream">The stream that the index was written to (optional)</param>
        //public static IDataTable CreateDataTable(Stream dataStream, Stream indexStream = null)
        //{
        //    if (indexStream == null)
        //        return DataTable.Create(dataStream, null);
        //    else
        //        return DataTable.Create(dataStream, indexStream, null);
        //}

        /// <summary>
        /// Creates a data table builder to programatically create data tables
        /// </summary>
        //public static IDataTableBuilder CreateDataTableBuilder(Stream stream = null, bool validate = true)
        //{
        //    return new DataTableWriter(stream, validate);
        //}

        /// <summary>
        /// Creates a data table builder to programatically create data tables
        /// </summary>
        //public static IDataTableBuilder CreateDataTableBuilder(IEnumerable<IColumn> columns, Stream stream = null, bool validate = true)
        //{
        //    return new DataTableWriter(columns, stream, validate);
        //}

        /// <summary>
        /// Create a markov model trainer of window size 2
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer2<T> CreateMarkovTrainer2<T>(int minObservations = 1)
        {
            return new MarkovModelTrainer2<T>(minObservations);
        }

        /// <summary>
        /// Create a markov model trainer of window size 3
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer3<T> CreateMarkovTrainer3<T>(int minObservations = 1)
        {
            return new MarkovModelTrainer3<T>(minObservations);
        }

		/// <summary>
		/// Returns a generic type converter that uses a default value if conversion fails
		/// </summary>
		/// <typeparam name="T">Type to conver to</typeparam>
		/// <param name="defaultValue">Value to use if the conversion fails</param>
	    //public static IConvertToType CreateTypeConverter<T>(T defaultValue = default(T))
	    //{
		   // return new GenericConverter<T>(defaultValue);
	    //}
    }
}
