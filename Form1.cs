﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Project1
{
    public partial class Form1 : Form
    {
        private Algorithm system;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            // Build up stations and RF
            for (int i = 0; i < (Algorithm.MAX_ADD_STATIONS + Algorithm.MAX_MULT_STATIONS); i++)
            {
                this.resStationsDGV.Rows.Add();
                this.resStationsDGV.Rows[i].HeaderCell.Value = "RS" + i;
            }
            for (int i = 0; i < Algorithm.MAX_RAT_ENTRIES; i++)
            {
                this.ratTableDGV.Rows.Add();
                this.ratTableDGV.Rows[i].HeaderCell.Value = "RF" + i;
            }
            for (int i = 0; i < Algorithm.MAX_ROBS; i++)
            {
                this.robDGV.Rows.Add();
                this.robDGV.Rows[i].HeaderCell.Value = "ROB" + i;
            }
        }

        /// <summary>
        /// Opens file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            OpenFileDialog openDx = new OpenFileDialog();
            if (openDx.ShowDialog() == DialogResult.OK)
            {
                if (openDx.CheckFileExists)
                {
                    // Clear all forms
                    ClearSystem();

                    this.stepButton.Enabled = true;
                    this.goToButton.Enabled = true;
                    using (StreamReader reader = new StreamReader(openDx.OpenFile()))
                    {
                        // Get instructions and cycles
                        string instructionNumber = reader.ReadLine();
                        string cycleNumber = reader.ReadLine();
                        uint instructions;
                        uint.TryParse(instructionNumber, out instructions);
                        uint cycles;
                        uint.TryParse(cycleNumber, out cycles);

                        system = new Algorithm();
                        // Load instructions into Instruction Queue
                        Queue<int[]> instructionTokenArrays = new Queue<int[]>();
                        for (int i = 0; i < instructions; i++)
                        {
                            string[] instrTokens_str = reader.ReadLine().Split(' ');

                            int[] instrTokens = Array.ConvertAll(instrTokens_str, int.Parse);
                            instructionTokenArrays.Enqueue(instrTokens);                           
                        }
                        // Load values into RF
                        for (int i = 0; i < Algorithm.MAX_RAT_ENTRIES; i++)
                        {
                            string line = reader.ReadLine();
                            int rfVal;
                            int.TryParse(line, out rfVal);
                            RatEntry ratEntry = new RatEntry();
                            ratEntry.RegisterFile = rfVal;
                            ratEntry.RAT = Algorithm.DEFAULT_RAT;
                            system.RAT.Add(ratEntry);
                            AddValuesToRF(i, rfVal);
                        }

                        for (int i = 0; i < instructions; i++)
                        {
                            int[] instruction = instructionTokenArrays.Dequeue();
                            int rf1 = instruction[2];
                            int rf2 = instruction[3];
                            instruction[2] = system.RAT[instruction[2]].RegisterFile;
                            instruction[3] = system.RAT[instruction[3]].RegisterFile;

                            Instruction instr = new Instruction(instruction[0], instruction[1], instruction[2], instruction[3], rf1, rf2);
                            system.InstructionQueue.Enqueue(instr);
                            AddInstructionToDGV(instr);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Empties all tables of values
        /// </summary>
        private void ClearSystem()
        {
            this.textBox1.Text = "0";
            ClearRS();
            ClearRAT();
            ClearRF();
            ClearInstructionQueue();
            ClearROB();            

            if (system != null)
            {
                if (system.InstructionQueue != null)
                {
                    system.InstructionQueue.Clear();
                }
                if (system.RAT != null)
                {
                    system.RAT.Clear();
                }
                if (system.ROBs != null)
                {
                    system.ROBs.Clear();
                }
            }
        }

        public void ClearRS()
        {
            foreach (DataGridViewRow row in resStationsDGV.Rows)
            {
                foreach (DataGridViewTextBoxCell cell in row.Cells)
                {
                    cell.Value = null;
                }
            }
        }

        public void ClearInstructionQueue()
        {
            for (int i = instructionQueueDGV.Rows.Count; i > 0; i--)
            {
                instructionQueueDGV.Rows.RemoveAt(i - 1);
            }
        }

        public void ClearROB()
        {
            foreach (DataGridViewRow row in robDGV.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = null;
                }
            }
        }

        public void ClearRAT()
        {
            foreach (DataGridViewRow row in ratTableDGV.Rows)
            {
                row.Cells["ratRATCol"].Value = null;
            }
        }

        public void ClearRF()
        {
            foreach (DataGridViewRow row in ratTableDGV.Rows)
            {
                row.Cells["ratRFCol"].Value = null;
            }
        }

        /// <summary>
        /// Add instruction to Instruction Queue DataGridView
        /// </summary>
        /// <param name="instr">Instruction to be added</param>
        private void AddInstructionToDGV(Instruction instr)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.HeaderCell.Value = "I" + this.instructionQueueDGV.Rows.Count;
            for (int i = 0; i < 4; i++)
            {
                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                row.Cells.Add(cell);
            }
            row.Cells[0].Value = Instruction.GetOpString(instr.Op);
            row.Cells[1].Value = instr.DestReg;
            row.Cells[2].Value = instr.V1;
            row.Cells[3].Value = instr.V2;
            this.instructionQueueDGV.Rows.Add(row);
        }

        /// <summary>
        /// Set RF values
        /// </summary>
        /// <param name="index">Register</param>
        /// <param name="value">Value</param>
        private void AddValuesToRF(int index, int value)
        {
            this.ratTableDGV.Rows[index].Cells[0].Value = value;
            this.ratTableDGV.Rows[index].Cells[1].Value = "Empty";
        }

        /// <summary>
        /// Fill reservation station DataGridView
        /// </summary>
        /// <param name="station">Reservation station information to fill DataGridView</param>
        private void AddValuesToRS(ReservationStation station)
        {
            if (station != null)
            {
                // Should this be rob?
                DataGridViewRow row = this.resStationsDGV.Rows[station.Index];
                row.Cells["rsBusyCol"].Value = station.Busy;
                row.Cells["rsOpCol"].Value = Instruction.GetOpString(station.Op);
                row.Cells["rsVjCol"].Value = station.Vj;
                row.Cells["rsVkCol"].Value = station.Vk;
                if (station.Qj != -1)
                {
                    row.Cells["rsQjCol"].Value = "ROB" + station.Qj;
                    row.Cells["rsVjCol"].Value = null;
                }
                if (station.Qk != -1)
                {
                    row.Cells["rsQkCol"].Value = "ROB" + station.Qk;
                    row.Cells["rsVkCol"].Value = null;
                }
            }
        }
        
        /// <summary>
        /// Handles step button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepButton_Click(object sender, EventArgs e)
        {
            // Update cycle text box
            int cycle = int.Parse(this.textBox1.Text);
            cycle++;
            this.textBox1.Text = cycle.ToString();

            // Get destination register file index
            int RF = (int)Algorithm.DEFAULT_RAT;
            if (system.InstructionQueue.Count > 0)
            {
                RF = system.InstructionQueue.First().DestReg;
            }

            // Update RAT & RF after commit
            ReorderBuffer rob = system.Commit();
            if (rob != null)
            {
                CommitUpdateDGV(rob);
            }

            // Broadcast if any values are available
            ArithmeticStation mathStation = system.Broadcast();

            if (mathStation != null)
            {
                BroadcastUpdate(mathStation);
            }

            // Dispatch if any stations are ready
            ReservationStation dispatchStation = system.Dispatch();
            while (dispatchStation != null)
            {
                this.resStationsDGV.Rows[dispatchStation.Index].Cells["rsBusyCol"].Value = false;
                dispatchStation = system.Dispatch();
            }

            // Issue instructions if available
            int issuePointer = system.IssuePointer;
            ReservationStation issueStation = system.Issue();
            if (issueStation != null)
            {
                this.instructionQueueDGV.Rows.RemoveAt(0);
                AddValuesToRS(issueStation);
                this.robDGV.Rows[issuePointer].Cells["regCol"].Value = "RF" + RF;
                int issuePointerCellIndex = (int)((issuePointer + 1) % Algorithm.MAX_ROBS);
                this.robDGV.Rows[issuePointerCellIndex].Cells["issueCol"].Value = this.textBox1.Text;
                this.robDGV.Rows[issuePointer].Cells["exceptionCol"].Value = "0";
                this.ratTableDGV.Rows[RF].Cells["ratRATCol"].Value = "ROB" + issuePointer;
            }
        }

        /// <summary>
        /// Updates values in DataGridViews from broadcast
        /// </summary>
        /// <param name="values">Reservation Station, Value</param>
        private void BroadcastUpdate(ArithmeticStation mathStation)
        {
            if (!mathStation.Exception)
            {
                // Update Reservation Stations
                foreach (DataGridViewRow row in this.resStationsDGV.Rows)
                {
                    foreach (DataGridViewTextBoxCell cell in row.Cells)
                    {
                        if (cell.Value != null && cell.Value.Equals("ROB" + mathStation.CurrentROB))
                        {
                            if (cell.ColumnIndex.Equals(4))
                            {
                                cell.Value = null;
                                row.Cells[2].Value = mathStation.Result;
                            }
                            else if (cell.ColumnIndex.Equals(5))
                            {
                                cell.Value = null;
                                row.Cells[3].Value = mathStation.Result;
                            }
                        }
                    }
                }
            }
            else
            {
                this.robDGV.Rows[mathStation.CurrentROB].Cells[5].Value = 1;
            }
            this.robDGV.Rows[mathStation.CurrentROB].Cells[1].Value = mathStation.Result;
            this.robDGV.Rows[mathStation.CurrentROB].Cells[2].Value = true;
        }


        private void CommitUpdateDGV(ReorderBuffer rob)
        {
            if (!rob.Exception)
            {
                string currentRAT = this.ratTableDGV.Rows[rob.RegisterFile].Cells["ratRatCol"].Value.ToString();
                if (currentRAT.Equals("ROB" + rob.Index))
                {
                    this.ratTableDGV.Rows[rob.RegisterFile].Cells["ratRatCol"].Value = "RF" + rob.RegisterFile.ToString();
                }
                this.ratTableDGV.Rows[rob.RegisterFile].Cells["ratRFCol"].Value = rob.Value;

                int commitPointerCellIndex = (int)((rob.Index + 1) % Algorithm.MAX_ROBS);
                this.robDGV.Rows[commitPointerCellIndex].Cells["commitCol"].Value = this.textBox1.Text;
            }
            else
            {
                this.ClearInstructionQueue();
                this.ClearRAT();
                this.ClearROB();
                this.ClearRS();
                system.ClearRATonCommit();
                system.ClearROBonCommit();
                system.ClearRSonCommit();

                MessageBox.Show("Arithmetic station caused an exception.  ROB, reservation stations, RAT, and instruction queue has been cleared.");
            }
        }

        /// <summary>
        /// Exits application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void goToButton_Click(object sender, EventArgs e)
        {
            int currentCycle = int.Parse(this.textBox1.Text);
            int goToCycle = int.Parse(this.goToTextBox.Text);
            int cyclesToSkip = goToCycle - currentCycle;

            if (goToCycle > currentCycle)
            {
                for(int i = 0; i < cyclesToSkip; i++)
                {
                    stepButton_Click(null, null);
                }
            }
            else
            {
                MessageBox.Show("Go to cycle must be after current cycle.");
            }
        }        
    }
}
