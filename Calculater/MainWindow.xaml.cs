using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Calculater
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = false;
        private Stack<string> historyStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private Dictionary<string, CommandBase> commands = new Dictionary<string, CommandBase>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommands();

            foreach (UIElement el in MainRoot.Children)
            {
                if (el is Button button)
                {
                    button.Click += Button_Click;
                }
            }

            menuButton.Click += MenuButton_Click;
        }

        private void InitializeCommands()
        {
            commands["C"] = new ClearCommand();
            commands["CE"] = new UndoCommand();
            commands["↷"] = new RedoCommand();
            commands["⌫"] = new BackspaceCommand();
            commands["n²"] = new SquareCommand();
            commands["π"] = new PiCommand();
            commands["e"] = new ECommand();
            commands["ln"] = new LnCommand();
            commands["√"] = new SqrtCommand();
            commands["="] = new EvaluateCommand();
            commands["default"] = new AppendTextCommand();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            menuColumn.Width = isMenuOpen ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            isMenuOpen = !isMenuOpen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string content = (string)((Button)e.OriginalSource).Content;

            if (!commands.TryGetValue(content, out var command))
            {
                command = commands["default"];
            }

            command.Execute(this, content);
        }

        public void PushToHistory(string value) => historyStack.Push(value);
        public string PopFromHistory() => historyStack.Count > 0 ? historyStack.Pop() : "";
        public void ClearHistory() => historyStack.Clear();
        public void PushToRedo(string value) => redoStack.Push(value);
        public string PopFromRedo() => redoStack.Count > 0 ? redoStack.Pop() : "";
        public void ClearRedo() => redoStack.Clear();
        public string GetText() => textlabel.Text;
        public void SetText(string value) => textlabel.Text = value;
    }

    public abstract class CommandBase
    {
        public abstract void Execute(MainWindow window, string parameter);
    }

    public class ClearCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            window.SetText("");
            window.ClearHistory();
            window.ClearRedo();
        }
    }

    public class UndoCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            window.PushToRedo(window.GetText());
            window.SetText(window.PopFromHistory());
        }
    }

    public class RedoCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            var previousState = window.PopFromRedo();
            if (!string.IsNullOrEmpty(previousState))
            {
                window.PushToHistory(window.GetText());
                window.SetText(previousState);
            }
        }
    }

    public class BackspaceCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            var current = window.GetText();
            if (!string.IsNullOrEmpty(current))
            {
                window.PushToRedo(current);
                window.SetText(current.Substring(0, current.Length - 1));
            }
        }
    }

    public class SquareCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            if (double.TryParse(current, out double number) && number >= 0)
            {
                window.PushToRedo(current);
                window.SetText(Math.Pow(number, 2).ToString());
            }
            else
            {
                window.SetText("Error");
            }
        }
    }

    public class PiCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            if (double.TryParse(current, out double number))
                window.SetText((number * Math.PI).ToString());
            else
                window.SetText(current + Math.PI.ToString());
        }
    }

    public class ECommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            if (double.TryParse(current, out double number))
                window.SetText((number * Math.E).ToString());
            else
                window.SetText(current + Math.E.ToString());
        }
    }

    public class LnCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            if (double.TryParse(current, out double number) && number > 0)
                window.SetText(Math.Log(number).ToString());
            else
                window.SetText("Error");
        }
    }

    public class SqrtCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            if (double.TryParse(current, out double number) && number >= 0)
                window.SetText(Math.Sqrt(number).ToString());
            else
                window.SetText("Error");
        }
    }

    public class EvaluateCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            try
            {
                string expression = window.GetText()
                    .Replace("×", "*")
                    .Replace(",", ".")
                    .Replace("÷", "/");

                if (expression.Contains("/0"))
                    throw new Exception();

                var result = new DataTable().Compute(expression, null).ToString();
                window.PushToHistory(window.GetText());
                window.ClearRedo();

                if (result.Length > 27)
                    window.SetText(Convert.ToDouble(result).ToString("E"));
                else
                    window.SetText(result);
            }
            catch
            {
                window.SetText("Error");
            }
        }
    }

    public class AppendTextCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);

            string[] operators = { "+", "-", "×", "÷" };
            string lastNumber = current.Split(operators, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";

            if (parameter == "00" && current != "" && current != "-" && !current.EndsWith("00") && current != "00")
            {
                if (!current.EndsWith("-0") && !current.EndsWith(" 0"))
                    window.SetText(current + parameter);
            }
            else if (parameter == "0")
            {
                if (current == "-0")
                    return;

                if (current.StartsWith("-0") && !current.Contains(",") && current.All(c => c == '-' || c == '0'))
                    return;

                if (current != "0")
                    window.SetText(current + parameter);
            }
            else if (parameter == ",")
            {
                if (lastNumber != "" && !lastNumber.Contains(",") && char.IsDigit(current.Last()))
                    window.SetText(current + parameter);
            }
            else if ("×÷".Contains(parameter) && current != "" && !"+-×÷".Contains(current.Last()))
            {
                window.SetText(current + parameter);
            }
            else if (parameter == "+" && current != "" && !"+-×÷".Contains(current.Last()))
            {
                window.SetText(current + parameter);
            }
            else if (parameter == "-" && (current == "" || !"+-×÷".Contains(current.Last())))
            {
                window.SetText(current + parameter);
            }
            else if (char.IsDigit(parameter[0]) && current == "0")
            {
                window.SetText(parameter);
            }
            else if (char.IsDigit(parameter[0]))
            {
                window.SetText(current + parameter);
            }
        }
    }
}
