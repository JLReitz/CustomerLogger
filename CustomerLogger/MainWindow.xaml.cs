﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using System.Xml;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;

using CSV;
using Jira_REST;
using CustomerLogger.Pages;

namespace CustomerLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// This is the main process which runs the main window for this application.
    /// </summary>
    public partial class MainWindow:Window
    {
        //  Members ///////////////////////////////////////////////////////////////////////////////

        private AppointmentPage m_Appointment_Page;                 //Appointment check-in
        private AppointmentProblemPage m_AppointmentProblem_Page;   //Appointment type
        private StudentIDPage m_StudentId_Page;                     //Student ID sign-in page
        private ProblemPage m_WiProblemPage;                        //Walk-In Problem Page
        private SummaryPage m_WiSummaryPage;                        //Walk-in Summary Page

        // Constructors ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// Initializes the customer logger main process and begins session-timeout timers.
        /// </remarks>
        public MainWindow()
        {
            InitializeComponent();

            //Create pages
            CreatePages();

            //Navigate to the student sign-in page
            ContentFrame.Navigate(m_StudentId_Page);

            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigating += ContentFrame_Navigating;
        }

        //  Properties  ///////////////////////////////////////////////////////////////////////////

        public AppointmentPage AppointmentPage
        {
            get { return m_Appointment_Page; }
        }

        public AppointmentProblemPage AppointmentProblemPage
        {
            get { return m_AppointmentProblem_Page; }
        }

        public ProblemPage ProblemPage
        {
            get { return m_WiProblemPage; }
        }

        public StudentIDPage StudentIDPage
        {
            get {return m_StudentId_Page; }
        }

        public SummaryPage SummaryPage
        {
            get {return m_WiSummaryPage; }
        }

        //  Public Functions    ///////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Removes all backpage history from the program instance.
        /// </summary>
        private void RemoveBackHistory()
        {
            //Reset all pages
            StudentIDPage.Reset();
            AppointmentPage.Reset();
            ProblemPage.Reset();
            AppointmentProblemPage.Reset();
            SummaryPage.Reset();

            //Remove Windows back history
            JournalEntry entry = ContentFrame.NavigationService.RemoveBackEntry();

            while (entry != null)
                entry = ContentFrame.NavigationService.RemoveBackEntry();
        }

        /// <summary>
        /// Resets all page states and navigates back to the student ID page.
        /// </summary>
        public void Reset()
        {
            RemoveBackHistory();                    //Remove all backhistory
            CreatePages();                          //Recreate all pages to clear out data
            ContentFrame.Navigate(StudentIDPage);   //Navigate to the student ID page
        }

        /*
        /// <summary>
        /// Forms a ticket to be sent to Jira via the cougtech walk in email service.
        /// </summary>
        /// <param name="id">Users WSU NID.</param>
        /// <param name="prob">Problem type.</param>
        /// <param name="descr">The problem description.</param>
        /// <param name="isAppt">Boolean for if the ticket is an appointment.</param>
        /// <returns>0 if the ticket was submitted. -1 if the ticket submission failed.</returns>
        public int SendTicket_Email(string id, string prob, string descr, bool isAppt)
        {
            //New mail object
            MailMessage msg = new MailMessage();
            MailAddress mailFrom = new MailAddress("cougtech.walkin@wsu.edu" + "<" + wsuEmail + ">");
            MailAddress mailTo = new MailAddress("cougtech.walkin@wsu.edu");

            msg.To.Add(mailTo);
            msg.From = mailFrom;

            SmtpClient client = new SmtpClient("mail.wsu.edu");


            //Format the subject based on the ticket information
            if (StudentIDPage.IsTest)           //Test ticket
            {
                msg.Subject = "##CTtest : " + prob + " : " + id;
            }
            else if (StudentIDPage.IsQuickPick) //Quickpick ticket
            {
                msg.Subject = "##CTwi : " + StudentIDPage.QuickCode;
            }
            else if (isAppt)                    //Appointment ticket
            {
                msg.Subject = "##CTapt  :  " + prob + "  :  " + id + "  :  " + QueryFirstName(id);
            }
            else                                //Standard walk-in ticket
            {
                msg.Subject = "##CTwi  :  " + prob + "  :  " + id + "  :  " + QueryFirstName(id);
            }

            //Build the message body
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToLongTimeString() + "\n" + id + "\n\n" + prob + "\n\n" + descr);
            sb.AppendLine("\n\nVerify the Customer's information in the Reporter field.");
            sb.AppendLine("Assign to yourself, if not already assigned.");
            sb.AppendLine("Start work notes using Start Work.");
            sb.AppendLine("Finish with Close or Move.");
            msg.Body = sb.ToString();

            //Change the cursor
            Cursor = Cursors.AppStarting;

            //Send the message
            try 
            {
                client.Send(msg);
            }
            catch (Exception e) 
            {
                MessageBox.Show("Ticket failed to send\nPlease try again.");
                Cursor = Cursors.Arrow;
                return -1; //-1 for error
            }

            //Change the cursor back
            Cursor = Cursors.Arrow;

            return 0; //0 for success
        }   
        */

        //  Private Functions   ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        private void AdminWindow_Login()
        {
            AdminWindow adminWindow = new AdminWindow(this, Cougtech_CustomerLogger.Logging_En, Cougtech_CustomerLogger.Ticketing_Email_En, 
                                                        Cougtech_CustomerLogger.Ticketing_Jira_En);

            if (adminWindow.Authenticate())
                adminWindow.ShowDialog();
        }

        /// <summary>
        /// Handler for when the content frame has finished navigating.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content == m_StudentId_Page)  //If the current content is the student ID page
                RemoveBackHistory();                //Clear all back history once Student ID Page has finished loading. This is essentially a soft reset
        }

        /// <summary>
        /// Handler for when the content frame is currently navigating.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)    //If the content frame is navigating backwards
                SummaryPage.StopTimer();                        //Stop the summary page timer
        }

        /// <summary>
        /// Creates all pages required for submitting a walk-in ticket.
        /// </summary>
        private void CreatePages()
        {
            m_StudentId_Page = new StudentIDPage();
            m_StudentId_Page.PageFinished += PageFinished;

            m_Appointment_Page = new AppointmentPage();
            m_Appointment_Page.PageFinished += PageFinished;

            m_AppointmentProblem_Page = new AppointmentProblemPage();
            m_AppointmentProblem_Page.PageFinished += PageFinished;

            m_WiProblemPage = new ProblemPage();
            m_WiProblemPage.PageFinished += PageFinished;

            m_WiSummaryPage = new SummaryPage();
            m_WiSummaryPage.PageFinished += PageFinished;
        }

        /// <summary>
        /// Event handler for when each customer logger page is finished.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void PageFinished(object sender, EventArgs e)
        {
            if (ContentFrame.Content == StudentIDPage) //At the student ID page
            {
                if (StudentIDPage.IsAdmin)                  //If the user is accessing the admin page
                {
                    AdminWindow_Login();                        //Launch the admin window login process
                    RemoveBackHistory();                        //Remove all back history
                }
                else
                {
                    //Create new ticket for new customer
                    Cougtech_CustomerLogger.CreateCustomerTicket(StudentIDPage.Nid);

                    if (StudentIDPage.IsQuickPick)         //Else if the issue is a quick-pick
                    {
                        //Populate the summary page and navigate to it
                        SummaryPage.SetText(false, Cougtech_CustomerLogger.CustomerTicket.CustomerName, Cougtech_CustomerLogger.CustomerTicket.Nid,
                                            Cougtech_CustomerLogger.CustomerTicket.Problem, Cougtech_CustomerLogger.CustomerTicket.Description);
                        ContentFrame.Navigate(SummaryPage);
                        SummaryPage.StartTimer();           //Start summary page timer
                    }
                    else                                        //Else
                    {
                        AppointmentPage.Set_text(Cougtech_CustomerLogger.CustomerTicket.CustomerName);
                        ContentFrame.Navigate(AppointmentPage);     //Navigate to the the appointment page
                    }
                }
            }
            else if (ContentFrame.Content == AppointmentPage)               //At the appointment page
            {
                if (Cougtech_CustomerLogger.CustomerTicket.IsAppointment)   //If the customer has an appointment
                    ContentFrame.Navigate(AppointmentProblemPage);                 //Navigate to the appointment problem page
                else                                                         //Else
                    ContentFrame.Navigate(ProblemPage);                         //Navigate to the standard problem page
            }
            else if (ContentFrame.Content == ProblemPage) //At the problem page
            {
                //Populate the summary page and navigate to it
                SummaryPage.SetText(false, Cougtech_CustomerLogger.CustomerTicket.CustomerName, Cougtech_CustomerLogger.CustomerTicket.Nid,
                                    Cougtech_CustomerLogger.CustomerTicket.Problem, Cougtech_CustomerLogger.CustomerTicket.Description);
                SummaryPage.StartTimer();
                ContentFrame.Navigate(SummaryPage);
            }
            else if (ContentFrame.Content == AppointmentProblemPage) //At the appointment problem page
            {
                //Populate the summary page and navigate to it
                SummaryPage.SetText(true, Cougtech_CustomerLogger.CustomerTicket.CustomerName, Cougtech_CustomerLogger.CustomerTicket.Nid,
                                    Cougtech_CustomerLogger.CustomerTicket.Problem, Cougtech_CustomerLogger.CustomerTicket.Description);
                SummaryPage.StartTimer();
                ContentFrame.Navigate(SummaryPage);
            }
            else if(ContentFrame.Content == SummaryPage)
            {
                //Submit the customer ticket then return to the student ID page
                Cougtech_CustomerLogger.SubmitTicket();
                ContentFrame.Navigate(StudentIDPage);
            }
        }
    }
}
