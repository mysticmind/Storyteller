﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using StoryTeller.Grammars.Sets;

namespace StoryTeller.Model.Persistence.DSL
{
    public static class FixtureWriter
    {
        public static string Write(FixtureModel model)
        {
            var writer = new StringWriter();
            Write(model, writer);

            return writer.ToString();
        }

        public static void Write(FixtureModel model, TextWriter writer)
        {
            writer.WriteLine($"# {model.title}");
            writer.WriteLine();
            writer.WriteLine();

            var grammars = model.grammars.Where(x => x.key != "TODO").OrderBy(x => x.key);
            foreach (var grammar in grammars)
            {
                writeGrammar(model, writer, grammar);
            }
        }

        private static void writeGrammar(FixtureModel model, TextWriter writer, GrammarModel grammar)
        {
            writer.WriteLine($"## {grammar.key}");


            if (grammar is Sentence)
            {
                writeSentence((Sentence) grammar, writer);
            }

            if (grammar is Table)
            {
                writeTable((Table) grammar, writer);
            }

            if (grammar is EmbeddedSection)
            {
                writeEmbed((EmbeddedSection) grammar, writer);
            }

            if (grammar is Paragraph)
            {
                writeParagraph(grammar.As<Paragraph>(), model, writer);
            }

            writer.WriteLine();
            writer.WriteLine();
        }

        private static void writeParagraph(Paragraph paragraph, FixtureModel model, TextWriter writer)
        {
            writer.WriteLine($"### {paragraph.title}");

            var list = new List<GrammarModel>();

            for (int i = 0; i < paragraph.children.Length; i++)
            {
                var child = paragraph.children[i];
                if (model.FindGrammar(child.key) == null)
                {
                    if (child.key.IsEmpty())
                    {
                        child.key = $"{paragraph.key}#{i + 1}";
                    }

                    list.Add(child);
                }

                writer.WriteLine("* " + child.key);
            }

            writer.WriteLine();
            writer.WriteLine();

            foreach (var grammar in list)
            {
                writeGrammar(model, writer, grammar);
            }
        }

        private static void writeEmbed(EmbeddedSection grammar, TextWriter writer)
        {
            writer.WriteLine($"### {grammar.title}");
            writer.WriteLine($"embeds {grammar.fixture.key}");
        }

        private static void writeTable(Table grammar, TextWriter writer)
        {
            writer.WriteLine($"### {grammar.title}");
            var tableType = "table";
            if (grammar is SetVerification)
            {
                var set = grammar.As<SetVerification>();

                tableType = set.ordered ? "ordered-set" : "set";
            }


            writeCellTable(tableType, writer, grammar.cells);
        }

        private static void writeCellTable(string tableType, TextWriter writer, Cell[] cells)
        {
            var firstRow = new[] {tableType}.Concat(cells.Select(x => x.Key)).ToArray();

            var headers = new[] {"header"}.Concat(cells.Select(x => x.header)).ToArray();

            var defaults = new[] {"default"}.Concat(cells.Select(x => x.DefaultValue ?? string.Empty)).ToArray();
            var options = new[] {"options"}.Concat(cells.Select(x => Option.Write(x.options))).ToArray();
            var editors = new[] {"editor"}.Concat(cells.Select(x => x.editor ?? string.Empty)).ToArray();
            var results = new[] {"result"}.Concat(cells.Select(x => x.result.ToString())).ToArray();

            writeTable(new[] {firstRow, headers, defaults, options, editors, results}, writer);
        }

        public static void Write(FixtureModel model, string file)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            {
                var writer = new StreamWriter(stream);

                Write(model, writer);
            }

            var text = Write(model);
            new FileSystem().WriteStringToFile(file, text);
        }

        private static void writeSentence(Sentence sentence, TextWriter writer)
        {
            writer.WriteLine($"### {sentence.format}");

            if (sentence.cells.Any())
            {
                writeCellTable("sentence", writer, sentence.cells);
            }
        }


        private static void writeTable(string[][] rows, TextWriter writer)
        {
            var columnCount = rows[0].Length;
            var widths = new int[columnCount];
            for (var i = 0; i < widths.Length; i++)
                widths[i] = rows.Select(x => x[i]?.Length ?? 0).Max();

            foreach (var row in rows)
            {
                writer.Write("|");
                for (var i = 0; i < widths.Length; i++)
                {
                    writer.Write((row[i] ?? string.Empty).PadRight(widths[i]));
                    writer.Write("|");
                }
                writer.WriteLine();
            }
        }
    }
}