using PandocMark.Formatters;
using PandocMark.Parser;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PandocMark
{
    /// <summary>
    /// Contains methods for parsing and formatting PandocMark data.
    /// </summary>
    public static class PandocMarkConverter
    {
        private static Lazy<Assembly> _assembly = new Lazy<Assembly>(InitializeAssembly, LazyThreadSafetyMode.None);

        private static Assembly Assembly => _assembly.Value;

        private static Assembly InitializeAssembly() => typeof(PandocMarkConverter).Assembly;

        private static Lazy<Version> _version = new Lazy<Version>(InitializeVersion, LazyThreadSafetyMode.None);

        /// <summary>
        /// Gets the PandocMark package version number.
        /// Note that this might differ from the actual assembly version which is updated less often to
        /// reduce problems when upgrading the nuget package.
        /// </summary>
        public static Version Version
        {
            get
            {
                return _version.Value;
            }
        }

        private static Version InitializeVersion()
        {
            // System.Xml is not available so resort to string parsing.
            using (var stream = Assembly.GetManifestResourceStream("PandocMark.Properties.PandocMark.NET.nuspec"))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var i = line.IndexOf("<version>", StringComparison.Ordinal);
                    if (i == -1)
                        continue;

                    i += 9;
                    return new Version(line.Substring(i, line.IndexOf("</version>", StringComparison.Ordinal) - i));
                }
            }
            return null;
        }

        /// <summary>
        /// Performs the first stage of the conversion - parses block elements from the source and created the syntax tree.
        /// </summary>
        /// <param name="source">The reader that contains the source data.</param>
        /// <param name="settings">The object containing settings for the parsing process.</param>
        /// <returns>The block element that represents the document.</returns>
        /// <exception cref="ArgumentNullException">when <paramref name="source"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during block parsing.</exception>
        /// <exception cref="IOException">when error occur while reading the data.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)] 
        public static Syntax.Block ProcessStage1(TextReader source, PandocMarkSettings settings = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (settings is null)
                settings = PandocMarkSettings.Default;

            var cur = Syntax.Block.CreateDocument();
            var doc = cur;
            var line = new LineInfo(settings.TrackSourcePosition);

            try
            {
                var reader = new TabTextReader(source);
                reader.ReadLine(line);
                while (line.Line != null)
                {
                    BlockMethods.IncorporateLine(line, ref cur);
                    reader.ReadLine(line);
                }
            }
            catch(IOException)
            {
                throw;
            }
            catch(PandocMarkException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new PandocMarkException("An error occurred while parsing line " + line.ToString(), cur, ex);
            }

            try
            {
                do
                {
                    BlockMethods.Finalize(cur, line);
                    cur = cur.Parent;
                } while (cur != null);
            }
            catch (PandocMarkException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PandocMarkException("An error occurred while finalizing open containers.", cur, ex);
            }

            return doc;
        }

        /// <summary>
        /// Performs the second stage of the conversion - parses block element contents into inline elements.
        /// </summary>
        /// <param name="document">The top level document element.</param>
        /// <param name="settings">The object containing settings for the parsing process.</param>
        /// <exception cref="ArgumentException">when <paramref name="document"/> does not represent a top level document.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="document"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during inline parsing.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)] 
        public static void ProcessStage2(Syntax.Block document, PandocMarkSettings settings = null)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (document.Tag != Syntax.BlockTag.Document)
                throw new ArgumentException("The block element passed to this method must represent a top level document.", nameof(document));

            if (settings is null)
                settings = PandocMarkSettings.Default;

            try
            {
                BlockMethods.ProcessInlines(document, document.Document, settings);
            }
            catch(PandocMarkException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new PandocMarkException("An error occurred during inline parsing.", ex);
            }
        }

        /// <summary>
        /// Performs the last stage of the conversion - converts the syntax tree to HTML representation.
        /// </summary>
        /// <param name="document">The top level document element.</param>
        /// <param name="target">The target text writer where the result will be written to.</param>
        /// <param name="settings">The object containing settings for the formatting process.</param>
        /// <exception cref="ArgumentException">when <paramref name="document"/> does not represent a top level document.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="document"/> or <paramref name="target"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during formatting.</exception>
        /// <exception cref="IOException">when error occur while writing the data to the target.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)] 
        public static void ProcessStage3(Syntax.Block document, TextWriter target, PandocMarkSettings settings = null)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (document.Tag != Syntax.BlockTag.Document)
                throw new ArgumentException("The block element passed to this method must represent a top level document.", nameof(document));

            if (settings is null)
                settings = PandocMarkSettings.Default;

            try
            {
                switch (settings.OutputFormat)
                {
                    case OutputFormat.Html:
                        HtmlFormatterSlim.BlocksToHtml(target, document, settings);
                        break;
                    case OutputFormat.SyntaxTree:
                        Printer.PrintBlocks(target, document, settings);
                        break;
                    case OutputFormat.CustomDelegate:
                        if (settings.OutputDelegate is null)
                            throw new PandocMarkException("If `settings.OutputFormat` is set to `CustomDelegate`, the `settings.OutputDelegate` property must be populated.");
                        settings.OutputDelegate(document, target, settings);
                        break;
                    default:
                        throw new PandocMarkException("Unsupported value '" + settings.OutputFormat + "' in `settings.OutputFormat`.");
                }
            }
            catch (PandocMarkException)
            {
                throw;
            }
            catch(IOException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new PandocMarkException("An error occurred during formatting of the document.", ex);
            }
        }

        /// <summary>
        /// Parses the given source data and returns the document syntax tree. Use <see cref="ProcessStage3"/> to
        /// convert the document to HTML using the built-in converter.
        /// </summary>
        /// <param name="source">The reader that contains the source data.</param>
        /// <param name="settings">The object containing settings for the parsing and formatting process.</param>
        /// <exception cref="ArgumentNullException">when <paramref name="source"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during parsing.</exception>
        /// <exception cref="IOException">when error occur while reading or writing the data.</exception>
        public static Syntax.Block Parse(TextReader source, PandocMarkSettings settings = null)
        {
            if (settings is null)
                settings = PandocMarkSettings.Default;

            var document = ProcessStage1(source, settings);
            ProcessStage2(document, settings);
            return document;
        }

        /// <summary>
        /// Parses the given source data and returns the document syntax tree. Use <see cref="ProcessStage3"/> to
        /// convert the document to HTML using the built-in converter.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <param name="settings">The object containing settings for the parsing and formatting process.</param>
        /// <exception cref="ArgumentNullException">when <paramref name="source"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during parsing.</exception>
        /// <exception cref="IOException">when error occur while reading or writing the data.</exception>
        public static Syntax.Block Parse(string source, PandocMarkSettings settings = null)
        {
            if (source is null)
                return null;

            using (var reader = new StringReader(source))
                return Parse(reader, settings);
        }

        /// <summary>
        /// Converts the given source data and writes the result directly to the target.
        /// </summary>
        /// <param name="source">The reader that contains the source data.</param>
        /// <param name="target">The target text writer where the result will be written to.</param>
        /// <param name="settings">The object containing settings for the parsing and formatting process.</param>
        /// <exception cref="ArgumentNullException">when <paramref name="source"/> or <paramref name="target"/> is <see langword="null"/></exception>
        /// <exception cref="PandocMarkException">when errors occur during parsing or formatting.</exception>
        /// <exception cref="IOException">when error occur while reading or writing the data.</exception>
        public static void Convert(TextReader source, TextWriter target, PandocMarkSettings settings = null)
        {
            if (settings is null)
                settings = PandocMarkSettings.Default;

            var document = ProcessStage1(source, settings);
            ProcessStage2(document, settings);
            ProcessStage3(document, target, settings);
        }

        /// <summary>
        /// Converts the given source data and returns the result as a string.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <param name="settings">The object containing settings for the parsing and formatting process.</param>
        /// <exception cref="PandocMarkException">when errors occur during parsing or formatting.</exception>
        /// <returns>The converted data.</returns>
        public static string Convert(string source, PandocMarkSettings settings = null)
        {
            if (source is null)
                return null;

            using (var reader = new StringReader(source))
            using (var writer = new StringWriter(System.Globalization.CultureInfo.CurrentCulture))
            {
                Convert(reader, writer, settings);

                return writer.ToString();
            }
        }
    }
}
