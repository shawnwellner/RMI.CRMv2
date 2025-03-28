using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Domain {
    public static class ContentCopy {
        #region InvalidCredentialsCopy
        public const string InvalidCredentialsCopy = @"
<b>Invalid username or password.</b>
</br>
Please try again.";
        #endregion

        #region NoCustomerRecords
        public const string NoCustomerRecords = @"<strong>There are currently no qualified records for {0}.<strong>";
        #endregion

        #region NotQualifiedScript
        public const string NotQualified = @"
After performing a search through our database, unfortunately we do not have any surgery facilities in your area that meet your criteria in our network and our standards at this time. One reason why is that we maintain a strict code in accepting facilities into our program.  Minimally invasive Surgery centers are always approaching us to partner and may have a facility near you in the future. We do have a number of doctors that are close to partnering with us but I don’t yet have any information on them. Why don’t I contact you again when I do?</br>
</br>
Yes/No</br>
</br>
If yes: I’ve made a notation of your record and we’ll be contacting you when a new facility can address your pain.  Thank you for inquiring about spine surgery facilities in your area. Thanks for your time today.";
        #endregion

        #region LoginModalCopy
        public const string LoginModalCopy = @"
<div>
    If you have setup an account with us, you can login with your assigned credentials. If you do not have an account or If you have 
    forgotten your username or password, please contact your account representative.
</div>
<ul class='modalcopy'>
    <li><strong>UserName: </strong>The username that was created with your account.</strong> </li>
    <li><strong>Password :</strong>The password that was created with your account.</li>
</ul>";
        #endregion LoginModalCopy

        #region Create Copy
        public const string CreateModalCopy = @"
<div>Use the defenitions below to help you fill out the Client Create form.</div>
<ul class='modalcopy'>
    <li><strong>Company Name :</strong> The name of the company for this client. Please use the complete company name i.e. ""LLC."", "".INC."". </li>
    <li><strong>Website Address :</strong> The url of the client company include "".com"", "".net"", "".org"" etc... </li>
    <li><strong>Company Division :</strong> If the client has more than one division please enter the specific division here.</li>
    <li><strong>Forwarding Number :</strong> The number call centers will use for the warm transfer.</li>
</ul>";

        public const string CreateClientModalCopy = @"
<div>Use the defenitions below to help you fill out the Client Create form.</div>
<ul class='modalcopy'>
    <li><strong>Username :</strong> The username for the manager or associate being created. </li>
    <li><strong>Password :</strong> The password for the manager or associate being created. All passwords must be between 8-12 characters long. </li>
    <li><strong>Comfirm Password :</strong> Must match the passwor field.</li>
</ul>";

        public const string CreateCallCenterUserModalCopy = @"
<div>Use the defenitions below to help you fill out the Client Create form.</div>
<ul class='modalcopy'>
    <li><strong>Username :</strong> The username for the manager or associate being created. </li>
    <li><strong>Password :</strong> The password for the manager or associate being created. All passwords must be between 8-12 characters long. </li>
    <li><strong>Comfirm Password :</strong> Must match the passwor field.</li>
</ul>";
       
        public const string CreateCustomerModalCopy = @"
<div>
    This questionnaire is designed to understand the patient’s needs and desires in order to match a surgery facility.  
    Please use the script and probing questions to fully complete the questionnaire.  
    When complete, click “Get List of Providers” to execute a search for matching facilities and providers.
</div>";

        public const string ProviderMoreInfoCopy = @"
<div class='row'>
    <div class='col-md-6'>
        <div><strong>Company Name</strong>: <span>{Company}</span></div>
        <div><strong>Location</strong>: <span>{Name}</span></div>
        <div><strong>Distance</strong>: <span>Aprox. {Distance} miles</span></div>
        <div><strong>Phone</strong>: <span>{ForwardPhone}</span></div>
        <div><strong>Phone2</strong>: <span>{ForwardPhone2}</span></div>
        <div><strong>Address</strong>: <span>{Address} {Address2}</span></div>
        <div><strong>City</strong>: <span>{City}</span></div>
        <div><strong>State</strong>: <span>{State}</span></div>
        <div><strong>Zip</strong>: <span>{Zip}</span></div>
        <div><strong>Website</strong>: <span><a href='{Website}' target='__blank'>{Website}</a></span></div>
    </div>
    <div id='googleMap'><div>
</div>";
        /*
        public const string CreateProviderModalCopy = @"
<div>
    Below is a list of all providers who are qualified to address the needs of the patient.  
    They are ordered from the most qualified to least qualified. Make every effort to refer and warm transfer the patient to the 1st result.  
    Subsequent results are to only be used if you have attempted and failed to contact and warm transfer the patient to the 1st result.
</div>
<div>
    <p>Instructions:</p>
    <ol>
        <li>Dial Warm Transfer number</li>
        <li>Click on Transfer Script button</li>
        <li>Follow the script with the other party</li>
        <li>If warm transfer is established, click “Successful” and “Save Customer Information”</li>
        <li>If warm transfer is NOT established.  Schedule a call back and click “UnSuccessful” and “Save Customer Information”.</li>
        <li>Call patient back at the appropriate time to complete warm transfer.</li>
    </ol>
</div>";
         */
        #endregion Create Copy

        #region Search Copy
        public const string SearchCustomerModalCopy = @"
<div>
    To search for an individual patient, type the Patient’s full or partial last name, phone number, and/or email address.  
    Click “Search for Patient” to perform the search.  The results will display below.
</div>
<ul class='modalcopy'>
    <li><strong>Last Name: </strong>The full last name of the person you are searching for. Or wildcard ""*word"",  ""word*"" </li>
    <li><strong>Phone :</strong> The phone number of the person you are searching for. Or wildcard ""*number"",  ""number*"" </li>
    <li><strong>Email Address :</strong>The email address of the person you are searching for. Or wildcard ""*word"",  ""word*""</li>
</ul>";

        public const string SearchClientModalCopy = @"
<div>
    You can search by Company Name, Phone, Email Address or Website address. 
    For a wild card use ""*"" before or after the word to be used.
</div>
<ul class='modalcopy'>
    <li><strong>Company Name : </strong>The full name of the company your are searching for. Or wildcard ""*word"",  ""word*"" </li>
    <li><strong>Phone :</strong> The phone number of the person you are searching for. Or wildcard ""*number"",  ""number*"" </li>
    <li><strong>Email Address :</strong> The email address of the person you are searching for. Or wildcard ""*word"",  ""word*""</li>
    <li><strong>Website :</strong> The url of the client company include "".com"", "".net"", "".org"" etc..</li>
</ul>";

        public const string SearchCallCenterUserModalCopy = @"
<div>
    You can search by Last Name, Phone Number or Email Address. For a wild card use ""*"" before or after the word to be used.
</div>
<ul class='modalcopy'>
    <li><strong>Last Name: </strong>The full last name of the person you are searching for. Or wildcard ""*word"",  ""word*"" </li>
    <li><strong>Phone :</strong> The phone number of the person you are searching for. Or wildcard ""*number"",  ""number*"" </li>
    <li><strong>Email Address :</strong>The email address of the person you are searching for. Or wildcard ""*word"",  ""word*""</li>
</ul>";
        #endregion Search Copy

        #region Update Copy
        public const string UpdateClientModalCopy = @"
<div>Use the defenitions below to help you fill out the Client Create form.</div>
<ul class='modalcopy'>
    <li><strong>Username :</strong> The username for the manager or associate being created. </li>
    <li><strong>Password :</strong> The password for the manager or associate being created. All passwords must be between 8-12 characters long. </li>
    <li><strong>Comfirm Password :</strong> Must match the passwor field.</li>
</ul>";

        public const string UpdateCustomerModalCopy = @"
<div>
    This questionnaire is designed to understand the patient’s needs and desires in order to match a surgery facility.  
    Please use the script and probing questions to fully complete the questionnaire.  
    When complete, click “Get List of Providers” to execute a search for matching facilities and providers.
</div>";

        public const string UpdateProviderModalCopy = @"
<div>
    Below is a list of all providers who are qualified to address the needs of the patient.  
    They are ordered from the most qualified to least qualified. Make every effort to refer and warm transfer the patient to the 1st result.  
    Subsequent results are to only be used if you have attempted and failed to contact and warm transfer the patient to the 1st result.
</div>
<div>
    <p>Instructions:</p>
    <ol>
        <li>Dial Warm Transfer number</li>
        <li>Click on Transfer Script button</li>
        <li>Follow the script with the other party</li>
        <li>If warm transfer is established, click “Successful” and “Save Customer Information”</li>
        <li>If warm transfer is NOT established.  Schedule a call back and click “UnSuccessful” and “Save Customer Information”.</li>
        <li>Call patient back at the appropriate time to complete warm transfer.</li>
    </ol>
</div>";

        public const string UpdateCallCenterUserModalCopy = @"
<div>Use the defenitions below to help you fill out the Client Create form.</div>
<ul class='modalcopy'>
    <li><strong>Username :</strong> The username for the manager or associate being created. </li>
    <li><strong>Password :</strong> The password for the manager or associate being created. All passwords must be between 8-12 characters long. </li>
    <li><strong>Comfirm Password :</strong> Must match the passwor field.</li>
</ul>";

        #endregion Update Copy

        #region No MRI Copy
        public const string NoMriModalCopy = @"
Since you have told me that you do not have an MRI or CT Scan, I won't be able to recommend a provider in your area.  
All our partners require an MRI or CT scan to effectively diagnose conditions and provide a solution for you.<br/>
<br/>
I can provide a website to help you find a facility that can perform an MRI:<br/>
<br/>
If yes:<br/>
&nbsp;&nbsp;&nbsp;http://www.acr.org/Quality-Safety/Accreditation/Accredited-Facility-Search
<br/>
Let me call you back 4-6 weeks from now to see if I can help you once you receive your MRI results.<br/>
<br/>
If no:<br/>
&nbsp;&nbsp;&nbsp;I'm sorry I couldn't help you further.  I hope you can find an alternative solution to your pain.";
        #endregion No MRI Copy

        #region CustomerScript
        
        public const string CustomerIncomingIntroScript = @"
<p>Thank you for calling about your back and neck pain my name is (associate name). Who do I have the pleasure of speaking to today?</p>
<p>
    <em>[Record name]</em>
</p>
<p>
    <span class='firstName'<em>Customer Name</em></span>, are you calling us today because you’re in chronic back pain and you’re looking for a possible solution through minimally invasive or non-invasive procedures.
</p>
<p>
    <em>
        Listen to customer</br>
        (refer back to their ailment to build rapport).
    </em>
</p>
<p>What I’d like to do is explain a little about us and how we can help you through our free service.</p>
<p>
    We are partners with a network of doctors and spine treatment facilities across the country who can potentially address your needs. They are board certified with decades 
    of experience in the orthopedics or neurology. Some have pioneered minimally invasive procedures; others have been featured in magazines and on Television such as in 
    Newsweek and on CBS and Fox.  Many have received awards and publish in medical journals.  Overall, our partners have about a 95% patient satisfaction rate.
</p>
<p>
    The majority of our partners perform outpatient procedures that only requires a tiny incision, sometime just a couple millimeters. The procedures are usually endoscopic, 
    which is accomplished through the use of lasers, radio frequency, or other minimally invasive tools.
</p>
<p>Have you considered this type of treatment before?</p>
<p>
    <strong>If yes:</strong> Great, than you know minimally invasive procedures are considered one of the safest and most effective options when you haven’t found relief 
    through other treatments. All I need to do is ask you a few simple questions, to see if I can locate a board certified doctor or accredited medical facility in our 
    network to assist you with your back or neck pain. Just to let you know again, our service to you is completely FREE!
</p>
<p>
    <strong>If no:</strong> Minimally invasive procedures are considered one of the safest and effective options around. Some people experience results immediately after 
    the procedure. Let me get some more information about you and I can see if there’s one of our partners that can help you further. Again, our service is completely FREE!
</p>
<p>Also be assured that your information is secured and will only be shared with a doctor or facility of your choice.</p>
<p>
    So, Customer Name, let me get some basic information from you.
</p>";

        public const string CustomerOutgoingIntroScript = @"
Hello, may I speak with <span class='fullName'<em>Customer Name</em></span>?</br>
</br>
Hi <span class='firstName'<em>Customer Name</em></span>, this is <em>(Associate Name)</em> with (BackPainRelief / LaserSpinalOperations).  I’m calling today because you had requested information for a solution 
to the back or neck pain you are experiencing. Have you found a solution to your back/neck already or already scheduled for treatment?</br>
</br>
<strong>If yes:</strong> Great, we’ll I’m glad to hear that you’ve found a solution to your pain. Of course if you would like to speak with us in the future about other spine procedures, 
feel free to contact us again. <em>(Terminate call)</em></br>
</br>
<strong>If no:</strong> Well, <span class='firstName'<em>Customer Name</em></span>, (BackPainRelief/LaserSpinalOperations) 
partners with a network of doctors and spine treatment facilities across the country to help you address the root cause of your pain, most often times through safe and effective minimally 
invasive procedures, as opposed to traditional open back surgery.  Normally procedures are out patient and only require a tiny incision.  The procedures are usually endoscopic, which is 
accomplished through the use of lasers, radio frequency, or other minimally invasive tools.</br>
</br>
All I need to do is ask you a few simple questions, to see if I can locate a board certified doctor or accredited medical facility in our network to assist you with your back or neck pain.  
Just to let you know, this service is <span class='underline-b'>completely FREE!</span></br>
</br>
Also be assured that your information is secured and will only be shared with a doctor or facility of your choice.</br>

<strong>If yes:</strong></br>
Great, I have some basic information from what you provided online.  Let me just verify it with you.";

        //{{FirstName | isempty:'(Customer Name)' }}
        public const string CustomerQuestionsScript = @"
<p>Ask for basic information:</p>
<div style='font-size:larger;'>
    <ul>
        <li>Name</li>
        <li>Phone Number (verify)</li>
        <li>Email address</li>
        <li>City</li>
        <li>State</li>
        <li>Zip</li>
    <ul>
</div>
<div style='padding-top:200px;'>
    <p>Could you tell me a little bit more about the back pain you’re experiencing?</p>
    <p>Have you been in this back pain for 6 months or longer?</p>
    <p>Have you seen an orthopedic or neurosurgeon about this pain?</p>
    <p>Have you had a MRI or CT scan (image or report) performed and do you have an image of it from within the last 3 years?</p>
    <p>Who is your primary Insurance provider?</p>
    <p>Can we contact again?</p>
</div>
";
        #endregion CustomerScript
    }
}