using System.Diagnostics;
using System.Windows.Input;

namespace AmorousEditor
{
    /// <summary>
    /// Interaction logic for ConsoleEmulation.xaml
    /// </summary>
    public partial class ConsoleEmulation
    {

        /// <summary>
        /// A new process
        /// </summary>
        private Process AmorousProcess;

        /// <summary>
        /// Found out Amorous is doing some console output, so I thought it might be useful to capture it
        /// </summary>
        /// <param name="amorous">The process to emulate console for</param>
        public ConsoleEmulation(Process amorous)
        {
            InitializeComponent();

            // Setting the Process to the passed parameter
            AmorousProcess = amorous;

            // Binds Output and Error events to the ConsoleWrite method
            AmorousProcess.OutputDataReceived += (s, e) => ConsoleWrite(e.Data);
            AmorousProcess.ErrorDataReceived += (s, e) => ConsoleWrite(e.Data);

            // Starts the process
            AmorousProcess.Start();

            // Starts reading lines from the Console output
            AmorousProcess.BeginErrorReadLine();
            AmorousProcess.BeginOutputReadLine();
        }
        
        /// <summary>
        /// Writes Console output text to dedicated TextBox
        /// </summary>
        /// <param name="text">String to add to the TextBox</param>
        private void ConsoleWrite(string text)
        {
            // Adds the string and a newline to the TextBox
            Dispatcher.Invoke(() => ConsoleText.Text += text + "\n" );
        }

        /// <summary>
        /// Checks if Enter key is pressed, and then sends input to Console (Or I hope it's Console)
        /// To be honest, I have no idea what this could be used for
        /// </summary>
        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            // Checks if the Key pressed is Return(Enter)
            if (e.Key == Key.Return)
            {
                // Writes entered text to StandardInput
                AmorousProcess.StandardInput.WriteLine(ConsoleInput.Text);

                // Clears the TextBox used for input
                ConsoleText.Text += ConsoleInput.Text + "\n";
                ConsoleInput.Text = "";
            }

            // Sets the event to Handled
            e.Handled = true;
        }
    }
}
