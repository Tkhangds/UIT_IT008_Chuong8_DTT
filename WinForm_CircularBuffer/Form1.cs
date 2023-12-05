using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForm_CircularBuffer
{
    public partial class Form1 : Form
    {
        //private System.Windows.Forms.TextBox outputTextBox;

        // required designer variable
        //private System.ComponentModel.Container component = null;

        public Form1()
        {
            InitializeComponent();
        }

    // Load event handler creates and starts threads
    private void CircularBuffer_Load(object sender, System.EventArgs e)
        {
            // create shared object
            outputTextBox.Text = "Khang Was Here";
            HoldIntegerSynchronized sharedLocation =
                new HoldIntegerSynchronized(outputTextBox);

            // display sharedLocation state before producer
            // and consumer threads begin execution
            outputTextBox.Text = sharedLocation.CreateStateOutput();

            // Random object used by each thread
            Random random = new Random();

            // create Producer and Consumer objects
            Producer producer = new Producer(sharedLocation, random, outputTextBox);
            Consumer consumer = new Consumer(sharedLocation, random, outputTextBox);
            Thread producerThread = new Thread(new ThreadStart(producer.Produce));
            producerThread.Name = "Producer";

            Thread consumerThread = new Thread(new ThreadStart(consumer.Consume));
            consumerThread.Name = "Consumer";

            // start threads
            producerThread.Start();
            consumerThread.Start();

        } // end CircularBuffer_Load method

    }
}

public class HoldIntegerSynchronized
    {
        // each array element is a buffer
        private int[] buffers = { -1, -1, -1 };

        // occupiedBufferCount maintains count of occupied buffers
        private int occupiedBufferCount = 0;

        // variable that maintains read and write buffer locations
        private int readLocation = 0, writeLocation = 0;

        // GUI component to display output
        private TextBox outputTextBox;

        // constructor
        public HoldIntegerSynchronized(TextBox output)
        {
            outputTextBox = output;
        }

        public int Buffer
        {
            get
            {
                // lock this object while getting value 
                // from buffers array
                lock (this)
                {
                    // if there is no data to read, place invoking 
                    // thread in WaitSleepJoin state
                    if (occupiedBufferCount == 0)
                    {
                        outputTextBox.Text += "\r\nAll buffers empty. " +
                            Thread.CurrentThread.Name + " waits.";
                        outputTextBox.ScrollToCaret();

                        Monitor.Wait(this);
                    }

                    // obtain value at the current readLocation, then 
                    // add a string indicating the consumed value to output
                    int readValue = buffers[readLocation];

                    outputTextBox.Text += "\r\n" +
                        Thread.CurrentThread.Name + " reads " +
                        buffers[readLocation] + " ";

                    // just consumed a value, so decrement the number of 
                    // occupied buffers
                    --occupiedBufferCount;
                    readLocation = (readLocation + 1) % buffers.Length;
                    outputTextBox.Text += CreateStateOutput();
                    outputTextBox.ScrollToCaret();

                    // return waiting thread (if there is one) 
                    // to Started state
                    Monitor.Pulse(this);

                    return readValue;

                } // end lock

            } // end accessor get

            set
            {
                // lock this object while setting value 
                // in buffers array
                lock (this)
                {
                    // if there are no empty locations, place invoking
                    // thread in WaitSleepJoin state
                    if (occupiedBufferCount == buffers.Length)
                    {
                        outputTextBox.Text += "\r\nAll buffers full. " +
                            Thread.CurrentThread.Name + " waits.";
                        outputTextBox.ScrollToCaret();

                        Monitor.Wait(this);
                    }
                    buffers[writeLocation] = value;

                    outputTextBox.Text += "\r\n" +
                        Thread.CurrentThread.Name + " writes " +
                        buffers[writeLocation] + " ";

                    // just produced a value, so increment the number of 
                    // occupied buffers
                    ++occupiedBufferCount;

                    // update writeLocation for future write operation,
                    // then add current state to output
                    writeLocation = (writeLocation + 1) % buffers.Length;
                    outputTextBox.Text += CreateStateOutput();
                    outputTextBox.ScrollToCaret();

                    // return waiting thread (if there is one) 
                    // to Started state
                    Monitor.Pulse(this);

                } // end lock

            } // end accessor set

        } // end property Buffer

        // create state output
        public string CreateStateOutput()
        {
            // display the first line of state information
            string output = "(buffers occupied: " +
                occupiedBufferCount + ")\r\nbuffers: ";
            for (int i = 0; i < buffers.Length; i++)
                output += " " + buffers[i] + " ";

            output += "\r\n";

            // display the second line of state information
            output += " ";

            for (int i = 0; i < buffers.Length; i++)
                output += "---- ";

            output += "\r\n";

            // display the third line of state information
            output += " ";

            // display readLocation (R) and writeLocation (W)
            // indicators below appropriate buffer locations
            for (int i = 0; i < buffers.Length; i++)
                if (i == writeLocation && writeLocation == readLocation)
                    output += " WR ";
                else if (i == writeLocation)
                    output += " W ";
                else if (i == readLocation)
                    output += " R ";
                else
                    output += " ";
            output += "\r\n";

            return output;
        }
    }

    public class Producer
    {
        private HoldIntegerSynchronized sharedLocation;
        private TextBox outputTextBox;
        private Random randomSleepTime;

        // constructor
        public Producer(HoldIntegerSynchronized shared, Random random, TextBox output)
        {
            sharedLocation = shared;
            outputTextBox = output;
            randomSleepTime = random;
        }

        // produce values from 11-20 and place them in 
        // sharedLocation's buffer
        public void Produce()
        {
            // sleep for a random interval up to 3000 milliseconds
            // then set sharedLocation's Buffer property
            for (int count = 11; count <= 20; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sharedLocation.Buffer = count;
            }
            string name = Thread.CurrentThread.Name;

            outputTextBox.Text += "\r\n" + name +
                " done producing.\r\n" + name + " terminated.\r\n";

            outputTextBox.ScrollToCaret();

        } // end method Produce

    }

    public class Consumer
    {
        private HoldIntegerSynchronized sharedLocation;
        private TextBox outputTextBox;
        private Random randomSleepTime;

        // constructor
        public Consumer(HoldIntegerSynchronized shared, Random random, TextBox output)
        {
            sharedLocation = shared;
            outputTextBox = output;
            randomSleepTime = random;
        }

        // consume 10 integers from buffer
        public void Consume()
        {
            int sum = 0;
            for (int count = 1; count <= 10; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sum += sharedLocation.Buffer;
            }

            string name = Thread.CurrentThread.Name;

            outputTextBox.Text += "\r\nTotal " + name +
                " consumed: " + sum + ".\r\n" + name +
                " terminated.\r\n";

            outputTextBox.ScrollToCaret();

        } // end method Consume

    } // end class Consumer



