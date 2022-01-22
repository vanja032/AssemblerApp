using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AssemblerApp
{
    public partial class Form1 : Form
    {
        List<string> instructionsRead = new List<string>();
        List<string> instructionsSave = new List<string>();
        Dictionary<int, int> relationships = new Dictionary<int, int>();
        Dictionary<string, int> predefinedValues = new Dictionary<string, int>();
        Dictionary<string, string> predefinedBinaryCompValues = new Dictionary<string, string>();
        Dictionary<string, string> predefinedBinaryDestinationValues = new Dictionary<string, string>();
        Dictionary<string, string> predefinedBinaryJumpValues = new Dictionary<string, string>();
        private void setPredefinedValues()
        {
            predefinedValues = new Dictionary<string, int>()
            {
                {"R0", 0 },
                {"R1", 1 },
                {"R2", 2 },
                {"R3", 3 },
                {"R4", 4 },
                {"R5", 5 },
                {"R6", 6 },
                {"R7", 7 },
                {"R8", 8 },
                {"R9", 9 },
                {"R10", 10 },
                {"R11", 11 },
                {"R12", 12 },
                {"R13", 13 },
                {"R14", 14 },
                {"R15", 15 },
                {"SP", 0 },
                {"LCL", 1 },
                {"ARG", 2 },
                {"THIS", 3 },
                {"THAT", 4 },
                {"SCREEN", 16384 },
                {"KBD", 24576 }
            };
            predefinedBinaryCompValues = new Dictionary<string, string>()
            {
                {"0", "0101010" },
                {"1", "0111111" },
                {"-1", "0111010" },
                {"D", "0001100" },
                {"A", "0110000" },
                {"!D", "0001101" },
                {"!A", "0110001" },
                {"-D", "0001111" },
                {"-A", "0110011" },
                {"D+1", "0011111" },
                {"1+D", "0011111" },
                {"A+1", "0110111" },
                {"1+A", "0110111" },
                {"D-1", "0001110" },
                {"-1+D", "0001110" },
                {"A-1", "0110010" },
                {"-1+A", "0110010" },
                {"D+A", "0000010" },
                {"A+D", "0000010" },
                {"D-A", "0010011" },
                {"-A+D", "0010011" },
                {"A-D", "0000111" },
                {"-D+A", "0000111" },
                {"D&A", "0000000" },
                {"A&D", "0000000" },
                {"D|A", "0010101" },
                {"A|D", "0010101" },
                {"M", "1110000" },
                {"!M", "1110001" },
                {"-M", "1110011" },
                {"M+1", "1110111" },
                {"1+M", "1110111" },
                {"M-1", "1110010" },
                {"-1+M", "1110010" },
                {"D+M", "1000010" },
                {"M+D", "1000010" },
                {"D-M", "1010011" },
                {"-M+D", "1010011" },
                {"M-D", "1000111" },
                {"-D+M", "1000111" },
                {"D&M", "1000000" },
                {"M&D", "1000000" },
                {"M|D", "1010101" },
                {"D|M", "1010101" }
            };
            predefinedBinaryDestinationValues = new Dictionary<string, string>()
            {
                {"null", "000" },
                {"M", "001" },
                {"D", "010" },
                {"DM", "011" },
                {"A", "100" },
                {"AM", "101" },
                {"AD", "110" },
                {"ADM", "111" }
            };
            predefinedBinaryJumpValues = new Dictionary<string, string>()
            {
                {"null", "000" },
                {"JGT", "001" },
                {"JEQ", "010" },
                {"JGE", "011" },
                {"JLT", "100" },
                {"JNE", "101" },
                {"JLE", "110" },
                {"JMP", "111" }
            };
        }
        private void clearDefaultValues()
        {
            predefinedValues.Clear();
            predefinedBinaryCompValues.Clear();
            predefinedBinaryDestinationValues.Clear();
            predefinedBinaryJumpValues.Clear();
            instructionsRead.Clear();
            instructionsSave.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Assembly Instructions File";
                dialog.Filter = "ASM Files (*.asm)|*.asm";
                
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    clearDefaultValues();
                    setPredefinedValues();
                    StreamReader reader = new StreamReader(dialog.FileName);
                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Replace(" ", string.Empty).ToUpper();
                        instructionsRead.Add(line);
                        listView1.Items.Add(new ListViewItem(line));
                    }
                    reader.Dispose();
                    reader.Close();
                    button2.Enabled = true;
                    relationships.Clear();
                    MessageBox.Show("Fajl je ispravno učitan.");
                }
                else
                    MessageBox.Show("Neuspešno čitanje fajla!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, int> customDefinedLabels = new Dictionary<string, int>();
                List<string> temp = new List<string>();
                int index = 0;
                int indexTemp = -1;
                foreach (string instruction in instructionsRead)
                {
                    indexTemp++;
                    string newInstruction = instruction;
                    if (instruction.Contains("//"))
                        newInstruction = instruction.Substring(0, instruction.IndexOf("//"));

                    if (newInstruction == "")
                        continue;
                    else if (newInstruction.IndexOf('(') == 0 && newInstruction.IndexOf(')') == newInstruction.Length - 1)
                    { customDefinedLabels.Add(newInstruction.Trim('(', ')'), index); continue; }
                    temp.Add(newInstruction);
                    relationships.Add(indexTemp, index);
                    index++;
                }
                int variableIndex = 16;
                foreach (string instruction in temp)
                {
                    if (instruction.IndexOf('@') != -1)
                    {
                        if (int.TryParse(instruction.Trim('@'), out int number))
                        {
                            if (number >= 0)
                            { instructionsSave.Add("0" + Convert.ToString(number, 2).PadLeft(15, '0')); }
                        }
                        else if (customDefinedLabels.Keys.Any(instruction.Trim('@').Contains))
                        { instructionsSave.Add("0" + Convert.ToString(customDefinedLabels[instruction.Trim('@')], 2).PadLeft(15, '0')); }
                        else if (predefinedValues.Keys.Any(instruction.Trim('@').Contains))
                        { instructionsSave.Add("0" + Convert.ToString(predefinedValues[instruction.Trim('@')], 2).PadLeft(15, '0')); }
                        else
                        {
                            instructionsSave.Add("0" + Convert.ToString(variableIndex, 2).PadLeft(15, '0'));
                            predefinedValues.Add(instruction.Trim('@'), variableIndex);
                            variableIndex++;
                        }
                    }
                    else
                    {
                        if (instruction.IndexOf('=') != -1)
                        {
                            instructionsSave.Add("111" + predefinedBinaryCompValues[instruction.Split('=')[1]] + predefinedBinaryDestinationValues[instruction.Split('=')[0]] + predefinedBinaryJumpValues["null"]);
                        }
                        else if (instruction.IndexOf(';') != -1)
                        {
                            instructionsSave.Add("111" + predefinedBinaryCompValues[instruction.Split(';')[0]] + predefinedBinaryDestinationValues["null"] + predefinedBinaryJumpValues[instruction.Split(';')[1]]);
                        }
                    }
                }
                foreach (string instruction in instructionsSave)
                {
                    listView2.Items.Add(new ListViewItem(instruction));
                }
                button3.Enabled = true;
                MessageBox.Show("Instrukcije su ispravno prevedene u mašinski kod.");
            }
            catch
            {
                MessageBox.Show("Instrukcije nisu ispravno prevedene u mašinski kod!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Save HACK Instructions File";
                dialog.Filter = "HACK Files (*.hack)|*.hack";

                if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                {
                    StreamWriter writer = new StreamWriter(dialog.FileName);
                    foreach (string instruction in instructionsSave)
                    {
                        writer.WriteLine(instruction);
                    }
                    writer.Dispose();
                    writer.Close();
                    MessageBox.Show("Uspešno sačuvan fajl.");
                }
                else
                    MessageBox.Show("Potrebno je uneti ispravan naziv fajla!");
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView2.SelectedIndices.Clear();
            foreach (ListViewItem item in listView1.Items)
                item.BackColor = Color.DarkSeaGreen;
            foreach (ListViewItem item in listView2.Items)
                item.BackColor = Color.DarkSeaGreen;
            foreach (int index in listView1.SelectedIndices)
                if (relationships.Keys.Contains(index))
                    listView2.Items[relationships[index]].BackColor = Color.FromArgb(255, 180, 51);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.SelectedIndices.Clear();
            foreach (ListViewItem item in listView1.Items)
                item.BackColor = Color.DarkSeaGreen;
            foreach (ListViewItem item in listView2.Items)
                item.BackColor = Color.DarkSeaGreen;
            foreach (int index in listView2.SelectedIndices)
                if (relationships.Values.Contains(index))
                    listView1.Items[index].BackColor = Color.FromArgb(255, 180, 51);
        }
    }
}
