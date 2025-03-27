using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Calculater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = false;
        private Stack<string> historyStack = new Stack<string>(); // Стек для збереження історії операцій
        private Stack<string> redoStack = new Stack<string>();
        public MainWindow()
        {
            InitializeComponent();
            foreach(UIElement el in MainRoot.Children)
            {
                if(el is Button)
                {
                    ((Button) el).Click += Button_Click;
                }
            }
            menuButton.Click += MenuButton_Click;
        }
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMenuOpen)
            {
                menuColumn.Width = new GridLength(0);
            }
            else
            {
                menuColumn.Width = new GridLength(1, GridUnitType.Star);

            }

            isMenuOpen = !isMenuOpen;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = (string)((Button)e.OriginalSource).Content;
            if (str == "00")
            {
                if (textlabel.Text != "" && textlabel.Text!="-" && textlabel.Text!="-0" && textlabel.Text!="00")
                {
                    textlabel.Text += str;
                }
            }
            else if(str == "0")
            {
                if (textlabel.Text != "-0")
                {
                    textlabel.Text += str;
                }
            }
            else if(str == ",")
            {
                if (textlabel.Text != "" && !textlabel.Text.Contains(",") && char.IsDigit(textlabel.Text[textlabel.Text.Length - 1]))
                {
                    textlabel.Text += str;
                }
            }
            else if (char.IsDigit(str[0]) && textlabel.Text == "0")
            {
                textlabel.Text = str;
            }
            else if(str == "⌫")
            {
                if (textlabel.Text.Length > 0)
                    textlabel.Text = textlabel.Text.Substring(0, textlabel.Text.Length - 1);
            }
            else if(str == "CE")
            {
                //return;
                if (historyStack.Count > 0)
                {
                    redoStack.Push(textlabel.Text); // Зберігаємо поточний вираз перед скасуванням
                    textlabel.Text = historyStack.Pop(); // Відновлюємо попередній вираз
                }
            }
            else if (str == "C")
            {
                //textlabel.Text = "";
                textlabel.Text = "";
                historyStack.Clear();
                redoStack.Clear();
            }
            else if (str == "n²")
            {
                double number;
                if (double.TryParse(textlabel.Text, out number) && number > 0)
                {
                    textlabel.Text = Math.Pow(number,2).ToString();
                }
                else
                {
                    textlabel.Text = "Error";
                }
            }
            else if(char.IsDigit(str[0]) && textlabel.Text == ",")
            {
                textlabel.Text += ",";
            }
            else if (str == "π")
            {
                double number;
                if (textlabel.Text!="" && double.TryParse(textlabel.Text, out number))
                {
                    textlabel.Text = (number*Math.PI).ToString();
                }
                else
                {
                    textlabel.Text += Math.PI.ToString();
                }
            }
            else if (str == "e")
            {
                double number;
                if (textlabel.Text != "" && double.TryParse(textlabel.Text, out number))
                {
                    textlabel.Text = (number * Math.E).ToString();
                }
                else
                {
                    textlabel.Text += Math.E.ToString();
                }
            }
            else if (str == "ln")
            {
                double number;
                if (double.TryParse(textlabel.Text, out number) && number > 0)
                {
                    textlabel.Text = Math.Log(number).ToString(); 
                }
                else
                {
                    textlabel.Text = "Error";
                }
            }
            else if (str == "√")
            {
                double number;
                if (double.TryParse(textlabel.Text, out number) && number >= 0)
                {
                    textlabel.Text = Math.Sqrt(number).ToString();
                }
                else
                {
                    textlabel.Text = "Error";
                }
            }
            else if(str == "+")
            {
                char[] operators = { '+', '-', '÷', '×' };
                if (textlabel.Text!="" && !operators.Contains(textlabel.Text.Last()))
                {
                    textlabel.Text += "+";
                }
            }
            else if (str == "-")
            {
                char[] operators = { '+', '-'};
                if (string.IsNullOrEmpty(textlabel.Text))
                {
                    textlabel.Text += "-";
                }
                else if (!operators.Contains(textlabel.Text.Last()))
                {
                    textlabel.Text += "-";
                }
            }
            else if (str == "×")
            {
                char[] operators = { '+', '-', '÷', '×' };
                if (textlabel.Text != "" && !operators.Contains(textlabel.Text.Last()))
                {
                    textlabel.Text += "×";
                }
            }
            else if (str == "÷")
            {
                char[] operators = { '+', '-', '÷', '×' };
                if (textlabel.Text != "" && !operators.Contains(textlabel.Text.Last()))
                {
                    textlabel.Text += "÷";
                }
            }
            else if (str == "≡")
            {
                return;
            }
            else if(str == "=")
            {
                try
                {
                    string expression = textlabel.Text.Replace("×", "*")
                                                      .Replace(",", ".")
                                                      .Replace("÷", "/");
                    if (expression.Contains("/0"))
                    {
                        throw new Exception();
                    }

                    string value = new DataTable().Compute(expression, null).ToString();

                    historyStack.Push(textlabel.Text); // Зберігаємо вираз перед обчисленням
                    redoStack.Clear(); // Після нового обчислення очищаємо redoStack

                    if (value.Length > 27)
                    {
                        double result = Convert.ToDouble(value);
                        textlabel.Text = result.ToString("E");
                    }
                    else
                    {
                        textlabel.Text = value;
                    }
                }
                catch (Exception)
                {
                    textlabel.Text = "Error";
                }
            }
            else
            {
                textlabel.Text += str;
            }
        }
    }
}
