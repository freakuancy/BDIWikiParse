Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Linq
Imports Aspose.Words
Imports Aspose.Words.Drawing



Public Class ParseTable
    ' Globals
    Private WORKDIR As String
    Private m_CharacterSheet As PlayerCharacter
    Private WithEvents bgw As New BackgroundWorker
    Private WithEvents bgwImage As New BackgroundWorker
    Private StrippedText As String
    Private strPath As String
    Private ItemCount As Integer

    Private Sub ParseTable_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set up propertgrid control
        m_CharacterSheet = New PlayerCharacter()
        PropertyGrid1.CommandsVisibleIfAvailable = True
        PropertyGrid1.Text = "Character"
        PropertyGrid1.SelectedObject = m_CharacterSheet
        ' Load settings
        chkDebug.Checked = My.Settings.Debug
        chkOnTop.Checked = My.Settings.Ontop
        chkDilemeter.Checked = My.Settings.UseDilemeter
        txtDilemeter.Text = My.Settings.Dilemeter
        WORKDIR = My.Settings.WorkingDir
        If WORKDIR = "" Then WORKDIR = Environment.CurrentDirectory
        txtImagePath.Text = WORKDIR

    End Sub
    Sub GridClear()
        'Clear Picture and PropertyGrid to empty
        m_CharacterSheet.Full_Name = ""
        m_CharacterSheet.Name = ""
        m_CharacterSheet.Race = ""
        m_CharacterSheet.Gender = ""
        m_CharacterSheet.Sexual_Orientation = ""
        m_CharacterSheet.Age = ""
        m_CharacterSheet.DOB = ""
        m_CharacterSheet.Country_Of_Origin = ""
        m_CharacterSheet.Hair = ""
        m_CharacterSheet.Eyes = ""
        m_CharacterSheet.Height = ""
        m_CharacterSheet.Weight = ""
        m_CharacterSheet.Nicknames_Or_Aliases = ""
        m_CharacterSheet.Religion = ""
        m_CharacterSheet.Alignment = ""
        m_CharacterSheet.Creed = ""
        m_CharacterSheet.Occupation = ""
        m_CharacterSheet.Income = ""
        m_CharacterSheet.Marital_Status = ""
        m_CharacterSheet.Player = ""
        m_CharacterSheet.Biography = ""
        m_CharacterSheet.Personality = ""
        m_CharacterSheet.Description = ""
        m_CharacterSheet.Abilities = ""
        m_CharacterSheet.Attributes = ""
        m_CharacterSheet.Mundane_Skills = ""
        m_CharacterSheet.Magic_Skills = ""
        m_CharacterSheet.Weapons = ""
        m_CharacterSheet.Armor = ""
        m_CharacterSheet.Tools = ""
        m_CharacterSheet.Goals = ""
        m_CharacterSheet.Picture = ""
        m_CharacterSheet.AltText = ""
        PropertyGrid1.Refresh()
        PictureBox1.Image = PictureBox1.InitialImage
    End Sub
    Function CleanLines(T As String)
        'Convert all line break types to vbCr/ASCII 13
        If String.IsNullOrEmpty(T) Then Return ""
        T = T.Replace(vbNewLine, vbCr).Replace(vbLf, vbCr)
        'Loop until all duplicate returns are removed
        Do While T.Contains(vbCr & vbCr)
                T = T.Replace(vbCr & vbCr, vbCr)
            Loop
        'Check to see if the string has one at the start to remove
        If T.StartsWith(vbCr) Then T = T.TrimStart(Chr(13))

        'Convert back to standard windows line breaks
        T = T.Replace(vbCr, vbNewLine + vbNewLine)
        Return T
    End Function
    Function StripTags(ByVal html As String) As String
        ' Remove HTML tags.
        html = Strings.Right(html, Len(html) - InStr(html, "Name:") + 1)
        html = Strings.Replace(html, "&nbsp;", " ")
        html = Strings.Replace(html, "&amp;", "&")
        html = Strings.Replace(html, "&rsquo;", "'")
        html = Strings.Replace(html, "&rdquo;", """")
        html = Strings.Replace(html, "&apos;", "'")
        html = Strings.Replace(html, "&quot;", """")
        html = Regex.Replace(html, "<.*?>", "")
        Return html
    End Function


    Private Sub bgw_DoWork(sender As Object, e As DoWorkEventArgs) Handles bgw.DoWork
        wikiParse(StrippedText)
    End Sub
    Private Sub bgwImage_DoWork(sender As Object, e As DoWorkEventArgs) Handles bgwImage.DoWork

        extractimages(strPath)
    End Sub
    Private Sub bgwImage_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles bgwImage.ProgressChanged

        If ToolStripProgressBar1.Value <= 95 Then ToolStripProgressBar1.PerformStep()
    End Sub
    Private Sub bgw_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles bgw.ProgressChanged

        ToolStripStatusLabel1.Text = "Parsing text..."

        PropertyGrid1.Refresh()
        If ToolStripProgressBar1.Value <= 95 Then ToolStripProgressBar1.PerformStep()
    End Sub
    Private Sub bgw_RunWorkerCompleted(sender As Object,
         e As RunWorkerCompletedEventArgs) Handles bgw.RunWorkerCompleted
        bgwImage.WorkerReportsProgress = True
        bgwImage.WorkerSupportsCancellation = True
        ToolStripStatusLabel1.Text = "Extracting Images..."
        PropertyGrid1.Refresh()
        bgwImage.RunWorkerAsync()
    End Sub
    Private Sub bgwImage_RunWorkerCompleted(sender As Object,
         e As RunWorkerCompletedEventArgs) Handles bgwImage.RunWorkerCompleted

        cmbImages.Items.Clear()
        cmbImages.Text = ""

        Dim imagefilename As String
        Dim imgFileName As String
        'Set up filename, derived from Full_name or just Name, 
        'strip spaces and specials

        imgFileName = m_CharacterSheet.Full_Name
        If imgFileName = "" Then imgFileName = m_CharacterSheet.Name
        imgFileName = Replace(imgFileName, " ", "")
        imgFileName = Regex.Replace(imgFileName, "[^A-Za-z0-9_. ]+", "")
        'Add files to combo
        For Each file As String In System.IO.Directory.GetFiles(WORKDIR + "\" + imgFileName)

            cmbImages.Items.Add(System.IO.Path.GetFileName(file))

        Next

        If cmbImages.Items.Count > 0 Then cmbImages.SelectedIndex = 0
        ToolStripProgressBar1.Visible = False
        If cmbImages.Items.Count = 0 Then
            Dim m As DialogResult = MessageBox.Show("No images were found in this sheet.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
        ToolStripStatusLabel1.Text = "Sheet parsed. There are " + ItemCount.ToString + " empty fields and " + cmbImages.Items.Count.ToString + " extracted images. "
        PropertyGrid1.Refresh()
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click

        ToolStripStatusLabel1.Text = "Parsing text..."
        ToolStripProgressBar1.Value = 0
        ToolStripProgressBar1.Visible = True

        ItemCount = 0
        ' Set up and show commondialog file browser
        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Filter = "Character files (*.rtf)|*.rtf|All files (*.*)|*.*"
        bgw.WorkerReportsProgress = True
        bgw.WorkerSupportsCancellation = True
        ToolStripProgressBar1.Visible = True
        Dim result As DialogResult = OpenFileDialog1.ShowDialog()
        If result = Windows.Forms.DialogResult.OK Then
            ToolStripProgressBar1.PerformStep()
            strPath = OpenFileDialog1.FileName
            Try
                'Read and parse rtf
                GridClear()
                Dim strFileContents As String = File.ReadAllText(strPath)
                strFileContents = strFileContents + vbNewLine
                HtmlRichTextBox1.Rtf = strFileContents
                StrippedText = StripTags(HtmlRichTextBox1.GetHTML(True, True))
                If chkDebug.Checked = True Then
                    If Not Application.OpenForms().OfType(Of frmMessage).Any Then
                        Dim Message As New frmMessage(HtmlRichTextBox1.GetHTML(True, True), "Debug")
                        Message.Show()
                    End If
                End If


                ToolStripProgressBar1.PerformStep()

                bgw.RunWorkerAsync()

                PropertyGrid1.Refresh()
                'Parse images


            Catch ex As Exception
                MsgBox("An error has occured:" + vbNewLine + vbNewLine + ex.Message.ToString)
            End Try
        End If
        'Final Control Refresh
        PropertyGrid1.Refresh()

    End Sub
    Sub extractimages(strPath As String)
        Dim doc As New Document(strPath)
        Dim shapes As NodeCollection = doc.GetChildNodes(NodeType.Shape, True)
        Dim imageindex As Integer = 1
        Dim shape As Shape
        Dim imagefilename As String
        Dim imgFileName As String
        'Set up filename
        imgFileName = m_CharacterSheet.Full_Name
        If imgFileName = "" Then imgFileName = m_CharacterSheet.Name
        imgFileName = Replace(imgFileName, " ", "")
        imgFileName = Regex.Replace(imgFileName, "[^A-Za-z0-9_. ]+", "")
        'Create folder for images
        My.Computer.FileSystem.CreateDirectory(WORKDIR + "\" + imgFileName)
        'Loop through SHAPES as defined by Apose library, save as image
        For Each shape In shapes
            If (shape.HasImage) Then
                bgwImage.ReportProgress(25)
                imagefilename = String.Format(imgFileName + "{0}{1}", imageindex, FileFormatUtil.ImageTypeToExtension(shape.ImageData.ImageType))
                shape.ImageData.Save(WORKDIR + "\" + imgFileName + "\" + imagefilename)
                imageindex = imageindex + 1

            End If

        Next
        'Delete leftover
        If imagefilename <> "" Then My.Computer.FileSystem.DeleteFile(WORKDIR + "\" + imgFileName + "\" + imagefilename)

    End Sub
    Private Sub wikiParse(strFileContents As String)
        Dim dilemeter As String = txtDilemeter.Text
        'Regexes for parsing
        If chkDilemeter.Checked = False Then dilemeter = ""
        Dim NameRegex As Regex = New Regex("Full Name: (.*)", RegexOptions.Multiline)
        Dim strName As Match = NameRegex.Match(strFileContents)
        m_CharacterSheet.Full_Name = Strings.LTrim(Strings.Replace(strName.Value.TrimEnd, "Full Name:", ""))
        m_CharacterSheet.AltText = Strings.LTrim(Strings.Replace(strName.Value, "Full Name:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Full_Name = "" Then ItemCount = ItemCount + 1

        Dim PartialNameRegex As Regex = New Regex("^Name: (.*)", RegexOptions.Multiline)
        Dim strPartialName As Match = PartialNameRegex.Match(strFileContents)
        m_CharacterSheet.Name = Strings.LTrim(Strings.Replace(strPartialName.Value.TrimEnd, "Name:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Name = "" Then ItemCount = ItemCount + 1

        Dim AliasRegex As Regex = New Regex("^Aliases or Nicknames: (.*)", RegexOptions.Multiline)
        Dim strAliases As Match = AliasRegex.Match(strFileContents)
        m_CharacterSheet.Nicknames_Or_Aliases = Strings.LTrim(Strings.Replace(strAliases.Value.TrimEnd, "Aliases or Nicknames:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Nicknames_Or_Aliases = "" Then ItemCount = ItemCount + 1

        Dim RaceRegex As Regex = New Regex("^Race: (.*)", RegexOptions.Multiline)
        Dim strRace As Match = RaceRegex.Match(strFileContents)
        m_CharacterSheet.Race = Strings.LTrim(Strings.Replace(strRace.Value.TrimEnd, "Race:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Race = "" Then ItemCount = ItemCount + 1

        Dim GenderRegex As Regex = New Regex("^Gender: (.*)", RegexOptions.Multiline)
        Dim strGender As Match = GenderRegex.Match(strFileContents)
        m_CharacterSheet.Gender = Strings.LTrim(Strings.Replace(strGender.Value.TrimEnd, "Gender:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Gender = "" Then ItemCount = ItemCount + 1

        Dim OrientationRegex As Regex = New Regex("^Sexual Orientation: (.*)", RegexOptions.Multiline)
        Dim strOrientation As Match = OrientationRegex.Match(strFileContents)
        m_CharacterSheet.Sexual_Orientation = Strings.LTrim(Strings.Replace(strOrientation.Value.TrimEnd, "Sexual Orientation:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Sexual_Orientation = "" Then ItemCount = ItemCount + 1

        Dim AgeRegex As Regex = New Regex("^Age: (.*)", RegexOptions.Multiline)
        Dim strAge As Match = AgeRegex.Match(strFileContents)
        m_CharacterSheet.Age = Strings.LTrim(Strings.Replace(strAge.Value.TrimEnd, "Age:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Age = "" Then ItemCount = ItemCount + 1

        Dim DOBRegex As Regex = New Regex("^Date of Birth: (.*)", RegexOptions.Multiline)
        Dim strDOB As Match = DOBRegex.Match(strFileContents)
        m_CharacterSheet.DOB = Strings.LTrim(Strings.Replace(strDOB.Value.TrimEnd, "Date of Birth:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.DOB = "" Then ItemCount = ItemCount + 1

        Dim OriginRegex As Regex = New Regex("^Country of Origin: (.*)", RegexOptions.Multiline)
        Dim strOrigin As Match = OriginRegex.Match(strFileContents)
        m_CharacterSheet.Country_Of_Origin = Strings.LTrim(Strings.Replace(strOrigin.Value.TrimEnd, "Country of Origin:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Country_Of_Origin = "" Then ItemCount = ItemCount + 1

        Dim HairRegex As Regex = New Regex("^Hair Color: (.*)", RegexOptions.Multiline)
        Dim strHair As Match = HairRegex.Match(strFileContents)
        m_CharacterSheet.Hair = Strings.LTrim(Strings.Replace(strHair.Value.TrimEnd, "Hair Color:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Hair = "" Then ItemCount = ItemCount + 1

        Dim EyeRegex As Regex = New Regex("^Eye Color: (.*)", RegexOptions.Multiline)
        Dim strEye As Match = EyeRegex.Match(strFileContents)
        m_CharacterSheet.Eyes = Strings.LTrim(Strings.Replace(strEye.Value.TrimEnd, "Eye Color:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Eyes = "" Then ItemCount = ItemCount + 1

        Dim HeightRegex As Regex = New Regex("^Height: (.*)", RegexOptions.Multiline)
        Dim strHeight As Match = HeightRegex.Match(strFileContents)
        m_CharacterSheet.Height = Strings.LTrim(Strings.Replace(strHeight.Value.TrimEnd, "Height:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Height = "" Then ItemCount = ItemCount + 1

        Dim WeightRegex As Regex = New Regex("^Weight: (.*)", RegexOptions.Multiline)
        Dim strWeight As Match = WeightRegex.Match(strFileContents)
        m_CharacterSheet.Weight = Strings.LTrim(Strings.Replace(strWeight.Value.TrimEnd, "Weight:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Weight = "" Then ItemCount = ItemCount + 1

        Dim ReligionRegex As Regex = New Regex("^Religion: (.*)", RegexOptions.Multiline)
        Dim strReligion As Match = ReligionRegex.Match(strFileContents)
        m_CharacterSheet.Religion = Strings.LTrim(Strings.Replace(strReligion.Value.TrimEnd, "Religion:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Religion = "" Then ItemCount = ItemCount + 1

        Dim AlignRegex As Regex = New Regex("^Alignment: (.*)", RegexOptions.Multiline)
        Dim strAlign As Match = AlignRegex.Match(strFileContents)
        m_CharacterSheet.Alignment = Strings.LTrim(Strings.Replace(strAlign.Value.TrimEnd, "Alignment:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Alignment = "" Then ItemCount = ItemCount + 1

        Dim CreedRegex As Regex = New Regex("^Creed: (.*)", RegexOptions.Multiline)
        Dim strCreed As Match = CreedRegex.Match(strFileContents)
        m_CharacterSheet.Creed = Strings.LTrim(Strings.Replace(strCreed.Value.TrimEnd, "Creed:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Creed = "" Then ItemCount = ItemCount + 1

        Dim OccupationRegex As Regex = New Regex("^Occupation: (.*)", RegexOptions.Multiline)
        Dim strOccupation As Match = OccupationRegex.Match(strFileContents)
        m_CharacterSheet.Occupation = Strings.LTrim(Strings.Replace(strOccupation.Value.TrimEnd, "Occupation:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Occupation = "" Then ItemCount = ItemCount + 1

        Dim IncomeRegex As Regex = New Regex("^Income: (.*)", RegexOptions.Multiline)
        Dim strIncome As Match = IncomeRegex.Match(strFileContents)
        m_CharacterSheet.Income = Strings.LTrim(Strings.Replace(strIncome.Value.TrimEnd, "Income:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Income = "" Then ItemCount = ItemCount + 1


        Dim MaritalRegex As Regex = New Regex("^Marital Status: (.*)", RegexOptions.Multiline)
        Dim strMarital As Match = MaritalRegex.Match(strFileContents)
        m_CharacterSheet.Marital_Status = Strings.LTrim(Strings.Replace(strMarital.Value.TrimEnd, "Marital Status:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Marital_Status = "" Then ItemCount = ItemCount + 1

        Dim PlayerRegex As Regex = New Regex("^Player: (.*)", RegexOptions.Multiline)
        Dim strPlayer As Match = PlayerRegex.Match(strFileContents)
        m_CharacterSheet.Player = Strings.LTrim(Strings.Replace(strPlayer.Value.TrimEnd, "Player:", ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Player = "" Then ItemCount = ItemCount + 1

        Dim BioRegex As Regex = New Regex("(?<=Biography" + dilemeter + ")\r?[\w\W]*?\r?(?=Personality" + dilemeter + ")", RegexOptions.Multiline)
        Dim strBiography As Match = BioRegex.Match(strFileContents)
        m_CharacterSheet.Biography = CleanLines(Strings.Replace(strBiography.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Biography = "" Then ItemCount = ItemCount + 1

        Dim PersonalityRegex As Regex = New Regex("(?<=Personality" + dilemeter + ")\r?[\w\W]*?\r?(?=Physical Description" + dilemeter + ")", RegexOptions.Multiline)
        Dim strPersonality As Match = PersonalityRegex.Match(strFileContents)
        m_CharacterSheet.Personality = CleanLines(Strings.Replace(strPersonality.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Personality = "" Then ItemCount = ItemCount + 1

        Dim DescriptionRegex As Regex = New Regex("(?<=Physical Description" + dilemeter + ")\r?[\w\W]*?\r?(?=Abilities & Skills" + dilemeter + ")", RegexOptions.Multiline)
        Dim strDescription As Match = DescriptionRegex.Match(strFileContents)
        m_CharacterSheet.Description = CleanLines(Strings.Replace(strDescription.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Description = "" Then ItemCount = ItemCount + 1

        Dim AbilitiesRegex As Regex = New Regex("(?<=Abilities & Skills" + dilemeter + ")\r?[\w\W]*?\r?(?=Physical Attributes" + dilemeter + ")", RegexOptions.Multiline)
        Dim strAbilities As Match = AbilitiesRegex.Match(strFileContents)
        m_CharacterSheet.Abilities = CleanLines(Strings.Replace(strAbilities.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Abilities = "" Then ItemCount = ItemCount + 1

        Dim AttributesRegex As Regex = New Regex("(?<=Physical Attributes" + dilemeter + ")\r?[\w\W]*?\r?(?=Mundane Skills" + dilemeter + ")", RegexOptions.Multiline)
        Dim strAttributes As Match = AttributesRegex.Match(strFileContents)
        m_CharacterSheet.Attributes = CleanLines(Strings.Replace(strAttributes.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Attributes = "" Then ItemCount = ItemCount + 1

        Dim MundaneRegex As Regex = New Regex("(?<=Mundane Skills" + dilemeter + ")\r?[\w\W]*?\r?(?=Magic Skills" + dilemeter + ")", RegexOptions.Multiline)
        Dim strMundane As Match = MundaneRegex.Match(strFileContents)
        m_CharacterSheet.Mundane_Skills = CleanLines(Strings.Replace(strMundane.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Mundane_Skills = "" Then ItemCount = ItemCount + 1

        Dim MagicRegex As Regex = New Regex("(?<=Magic Skills" + dilemeter + ")\r?[\w\W]*?\r?(?=Equipment" + dilemeter + ")", RegexOptions.Multiline)
        Dim strMagic As Match = MagicRegex.Match(strFileContents)
        m_CharacterSheet.Magic_Skills = CleanLines(Strings.Replace(strMagic.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Magic_Skills = "" Then ItemCount = ItemCount + 1

        Dim WeaponsRegex As Regex = New Regex("(?<=-Weapons" + dilemeter + ")\r?[\w\W]*?\r?(?=-Armor" + dilemeter + ")", RegexOptions.Multiline)
        Dim strWeapons As Match = WeaponsRegex.Match(strFileContents)
        m_CharacterSheet.Weapons = CleanLines(Strings.Replace(strWeapons.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Weapons = "" Then ItemCount = ItemCount + 1

        Dim ArmorRegex As Regex = New Regex("(?<=-Armor" + dilemeter + ")\r?[\w\W]*?\r?(?=-Tools" + dilemeter + ")", RegexOptions.Multiline)
        Dim strArmor As Match = ArmorRegex.Match(strFileContents)
        m_CharacterSheet.Armor = CleanLines(Strings.Replace(strArmor.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Armor = "" Then ItemCount = ItemCount + 1

        Dim ToolsRegex As Regex = New Regex("(?<=-Tools" + dilemeter + ")\r?[\w\W]*?\r?(?=Goals" + dilemeter + ")", RegexOptions.Multiline)
        Dim strTools As Match = ToolsRegex.Match(strFileContents)
        m_CharacterSheet.Tools = CleanLines(Strings.Replace(strTools.Value.TrimEnd.TrimStart, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Tools = "" Then ItemCount = ItemCount + 1

        Dim GoalString As String = strFileContents.Substring(strFileContents.IndexOf("Goals" + dilemeter) + (5 + Len(dilemeter)))
        m_CharacterSheet.Goals = CleanLines(Strings.Replace(GoalString.TrimStart.TrimEnd, vbNewLine + vbNewLine, ""))
        bgw.ReportProgress(5)
        If m_CharacterSheet.Goals = "" Then ItemCount = ItemCount + 1

    End Sub


    Private Sub PropertyGrid1_SelectedGridItemChanged(sender As Object, e As SelectedGridItemChangedEventArgs) Handles PropertyGrid1.SelectedGridItemChanged
        TextBox1.Text = PropertyGrid1.SelectedGridItem.Value
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If TextBox1.Text <> PropertyGrid1.SelectedGridItem.Value Then
            Label1.Text = "EDITED"
            Label1.ForeColor = Color.Red
        End If

    End Sub


    Public Class PlayerCharacter
        'Set up PropertyGrid members
        Private m_FullName As String
        Private m_Name As String
        Private m_Race As String
        Private m_Gender As String
        Private m_Orientation As String
        Private m_Age As String
        Private m_DOB As String
        Private m_Origin As String
        Private m_Hair As String
        Private m_Eyes As String
        Private m_Height As String
        Private m_Weight As String
        Private m_Nicknames As String
        Private m_Religion As String
        Private m_Alignment As String
        Private m_Creed As String
        Private m_Occupation As String
        Private m_Income As String
        Private m_Marital As String
        Private m_Player As String
        Private m_Bio As String
        Private m_Personality As String
        Private m_Description As String
        Private m_Abilities As String
        Private m_Attributes As String
        Private m_Mundane As String
        Private m_Magic As String
        Private m_Weapons As String
        Private m_Armor As String
        Private m_Tools As String
        Private m_Goals As String
        Private m_Picfile As String
        Private m_AltText As String


        'Define PropertyGrid members
        <Description(""), Category("Vitals")>
        Public Property Full_Name() As String
            Get
                Return m_FullName
            End Get
            Set(ByVal value As String)
                m_FullName = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Picture() As String
            Get
                Return m_Picfile
            End Get
            Set(ByVal value As String)
                m_Picfile = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property AltText() As String
            Get
                Return m_AltText
            End Get
            Set(ByVal value As String)
                m_AltText = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Nicknames_Or_Aliases() As String
            Get
                Return m_Nicknames
            End Get
            Set(ByVal value As String)
                m_Nicknames = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Race() As String
            Get
                Return m_Race
            End Get
            Set(ByVal value As String)
                m_Race = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Gender() As String
            Get
                Return m_Gender
            End Get
            Set(ByVal value As String)
                m_Gender = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Sexual_Orientation() As String
            Get
                Return m_Orientation
            End Get
            Set(ByVal value As String)
                m_Orientation = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Age() As String
            Get
                Return m_Age
            End Get
            Set(ByVal value As String)
                m_Age = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property DOB() As String
            Get
                Return m_DOB
            End Get
            Set(ByVal value As String)
                m_DOB = value
            End Set
        End Property

        <Description(""), Category("Vitals")>
        Public Property Hair() As String
            Get
                Return m_Hair
            End Get
            Set(ByVal value As String)
                m_Hair = value
            End Set
        End Property

        <Description(""), Category("Vitals")>
        Public Property Eyes() As String
            Get
                Return m_Eyes
            End Get
            Set(ByVal value As String)
                m_Eyes = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Weight() As String
            Get
                Return m_Weight
            End Get
            Set(ByVal value As String)
                m_Weight = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Height() As String
            Get
                Return m_Height
            End Get
            Set(ByVal value As String)
                m_Height = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Country_Of_Origin() As String
            Get
                Return m_Origin
            End Get
            Set(ByVal value As String)
                m_Origin = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Religion() As String
            Get
                Return m_Religion
            End Get
            Set(ByVal value As String)
                m_Religion = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Alignment() As String
            Get
                Return m_Alignment
            End Get
            Set(ByVal value As String)
                m_Alignment = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Creed() As String
            Get
                Return m_Creed
            End Get
            Set(ByVal value As String)
                m_Creed = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Occupation() As String
            Get
                Return m_Occupation
            End Get
            Set(ByVal value As String)
                m_Occupation = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Income() As String
            Get
                Return m_Income
            End Get
            Set(ByVal value As String)
                m_Income = value
            End Set
        End Property
        <Description(""), Category("Demographics")>
        Public Property Marital_Status() As String
            Get
                Return m_Marital
            End Get
            Set(ByVal value As String)
                m_Marital = value
            End Set
        End Property
        <Description(""), Category("Vitals")>
        Public Property Player() As String
            Get
                Return m_Player
            End Get
            Set(ByVal value As String)
                m_Player = value
            End Set
        End Property
        <Description(""), Category("Player Details")>
        Public Property Biography() As String
            Get
                Return m_Bio
            End Get
            Set(ByVal value As String)
                m_Bio = value
            End Set
        End Property
        <Description(""), Category("Player Details")>
        Public Property Personality() As String
            Get
                Return m_Personality
            End Get
            Set(ByVal value As String)
                m_Personality = value
            End Set
        End Property
        <Description(""), Category("Player Details")>
        Public Property Description() As String
            Get
                Return m_Description
            End Get
            Set(ByVal value As String)
                m_Description = value
            End Set
        End Property
        <Description(""), Category("Player Details")>
        Public Property Attributes() As String
            Get
                Return m_Attributes
            End Get
            Set(ByVal value As String)
                m_Attributes = value
            End Set
        End Property
        <Description(""), Category("Abilities and Skills")>
        Public Property Abilities() As String
            Get
                Return m_Abilities
            End Get
            Set(ByVal value As String)
                m_Abilities = value
            End Set
        End Property
        <Description(""), Category("Abilities and Skills")>
        Public Property Mundane_Skills() As String
            Get
                Return m_Mundane
            End Get
            Set(ByVal value As String)
                m_Mundane = value
            End Set
        End Property
        <Description(""), Category("Abilities and Skills")>
        Public Property Magic_Skills() As String
            Get
                Return m_Magic
            End Get
            Set(ByVal value As String)
                m_Magic = value
            End Set
        End Property
        <Description(""), Category("Equipment")>
        Public Property Weapons() As String
            Get
                Return m_Weapons
            End Get
            Set(ByVal value As String)
                m_Weapons = value
            End Set
        End Property
        <Description(""), Category("Equipment")>
        Public Property Armor() As String
            Get
                Return m_Armor
            End Get
            Set(ByVal value As String)
                m_Armor = value
            End Set
        End Property
        <Description(""), Category("Equipment")>
        Public Property Tools() As String
            Get
                Return m_Tools
            End Get
            Set(ByVal value As String)
                m_Tools = value
            End Set
        End Property
        <Description(""), Category("Misc.")>
        Public Property Goals() As String
            Get
                Return m_Goals
            End Get
            Set(ByVal value As String)
                m_Goals = value
            End Set
        End Property
    End Class

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        'Save to PropertyGrid. This is a little ugly but necessary to avoid complicating
        'the code with Reflection

        Label1.Text = "SAVED"
        Label1.ForeColor = Color.Green
        If PropertyGrid1.SelectedGridItem.Label = "Full_Name" Then m_CharacterSheet.Full_Name = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Picture" Then m_CharacterSheet.Picture = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "AltText" Then m_CharacterSheet.AltText = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Name" Then m_CharacterSheet.Name = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Race" Then m_CharacterSheet.Race = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Gender" Then m_CharacterSheet.Gender = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Sexual_Orientation" Then m_CharacterSheet.Sexual_Orientation = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Age" Then m_CharacterSheet.Age = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "DOB" Then m_CharacterSheet.DOB = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Country_Of_Origin" Then m_CharacterSheet.Country_Of_Origin = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Hair" Then m_CharacterSheet.Hair = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Eyes" Then m_CharacterSheet.Eyes = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Height" Then m_CharacterSheet.Height = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Weight" Then m_CharacterSheet.Weight = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Nicknames_Or_Aliases" Then m_CharacterSheet.Nicknames_Or_Aliases = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Religion" Then m_CharacterSheet.Religion = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Alignment" Then m_CharacterSheet.Alignment = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Creed" Then m_CharacterSheet.Creed = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Occupation" Then m_CharacterSheet.Occupation = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Income" Then m_CharacterSheet.Income = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Marital_Status" Then m_CharacterSheet.Marital_Status = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Player" Then m_CharacterSheet.Player = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Biography" Then m_CharacterSheet.Biography = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Personality" Then m_CharacterSheet.Personality = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Description" Then m_CharacterSheet.Description = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Attributes" Then m_CharacterSheet.Attributes = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Abilities" Then m_CharacterSheet.Abilities = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Mundane_Skills" Then m_CharacterSheet.Mundane_Skills = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Magic_Skills" Then m_CharacterSheet.Magic_Skills = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Weapons" Then m_CharacterSheet.Weapons = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Armor" Then m_CharacterSheet.Armor = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Tools" Then m_CharacterSheet.Tools = TextBox1.Text
        If PropertyGrid1.SelectedGridItem.Label = "Goals" Then m_CharacterSheet.Goals = TextBox1.Text
        Me.ActiveControl = PropertyGrid1
        PropertyGrid1.Refresh()
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

        TextBox1.Clear()

    End Sub

    Private Sub btngenerate_Click(sender As Object, e As EventArgs) Handles btngenerate.Click
        'Set up WikiTable output
        Dim strSheetHeader As String = "{{Infobox Sheet"
        Dim strName As String = "| name              = " + m_CharacterSheet.Name
        Dim strPicFile As String = "| picfile           = " + m_CharacterSheet.Picture
        Dim strAltText As String = "| alttext           = " + m_CharacterSheet.AltText
        Dim strFullName As String = "| full_name         = " + m_CharacterSheet.Full_Name
        Dim strRace As String = "| race              = " + m_CharacterSheet.Race
        Dim strGender As String = "| gender              = " + m_CharacterSheet.Gender
        Dim strOrientation As String = "| orientation              = " + m_CharacterSheet.Sexual_Orientation
        Dim strAge As String = "| age              = " + m_CharacterSheet.Age
        Dim strDOB As String = "| date_of_birth              = " + m_CharacterSheet.DOB
        Dim strOrigin As String = "| country_of_origin              = " + m_CharacterSheet.Country_Of_Origin
        Dim strHair As String = "| hair_color              = " + m_CharacterSheet.Hair
        Dim strEyes As String = "| eye_color              = " + m_CharacterSheet.Eyes
        Dim strHeight As String = "| height              = " + m_CharacterSheet.Height
        Dim strWeight As String = "| weight              = " + m_CharacterSheet.Weight
        Dim strAliases As String = "| aliases              = " + m_CharacterSheet.Nicknames_Or_Aliases
        Dim strReligion As String = "| religion              = " + m_CharacterSheet.Religion
        Dim strCreed As String = "| alignment              = " + m_CharacterSheet.Alignment
        Dim strAlignment As String = "| creed              = " + m_CharacterSheet.Creed
        Dim strOccupation As String = "| occupation              = " + m_CharacterSheet.Occupation
        Dim strIncome As String = "| income              = " + m_CharacterSheet.Income
        Dim strMarital As String = "| marital_status              = " + m_CharacterSheet.Marital_Status
        Dim strPlayer As String = "| player              = " + m_CharacterSheet.Player
        Dim strSheetFooter As String = "}}"
        Dim strBiography As String = "=Biography=" + vbNewLine + CleanLines(m_CharacterSheet.Biography) + vbNewLine + vbNewLine
        Dim strPersonality As String = "=Personality=" + vbNewLine + CleanLines(m_CharacterSheet.Personality) + vbNewLine + vbNewLine
        Dim strDescription As String = "=Physical Description=" + vbNewLine + CleanLines(m_CharacterSheet.Description) + vbNewLine + vbNewLine
        Dim strAbilities As String = "=Abilities & Skills=" + vbNewLine + vbNewLine
        Dim strPhysical As String = "==Physical Abilities==" + vbNewLine + CleanLines(m_CharacterSheet.Abilities) + vbNewLine + vbNewLine
        Dim strMundane As String = "==Mundane Skills==" + vbNewLine + CleanLines(m_CharacterSheet.Mundane_Skills) + vbNewLine + vbNewLine
        Dim strMagic As String = "==Magic Skills==" + vbNewLine + CleanLines(m_CharacterSheet.Magic_Skills) + vbNewLine + vbNewLine
        Dim strEquipment As String = "=Equipment=" + vbNewLine + vbNewLine + vbNewLine
        Dim strWeapons As String = "==Weapons==" + vbNewLine + CleanLines(m_CharacterSheet.Weapons) + vbNewLine + vbNewLine
        Dim strArmor As String = "==Armor==" + vbNewLine + CleanLines(m_CharacterSheet.Armor) + vbNewLine + vbNewLine
        Dim strTools As String = "==Tools==" + vbNewLine + CleanLines(m_CharacterSheet.Tools) + vbNewLine + vbNewLine
        Dim strGoals As String = "=Goals=" + vbNewLine + CleanLines(m_CharacterSheet.Goals)
        Dim strFooter As String = "[[Category: Player Characters]]"
        'Build Strings
        Dim strLongForm As String = strBiography + strPersonality + strDescription + strAbilities + strPhysical + strMundane + strMagic + strEquipment + strWeapons + strArmor + strTools + strGoals
        Dim strWikiString As String = strSheetHeader + vbNewLine + strName + vbNewLine + strPicFile + vbNewLine + strAltText + vbNewLine + strFullName + vbNewLine + strRace + vbNewLine + strGender + vbNewLine + strOrientation + vbNewLine + strAge + vbNewLine + strDOB + vbNewLine + strOrigin + vbNewLine + strHair + vbNewLine + strEyes + vbNewLine + strHeight + vbNewLine + strWeight + vbNewLine + strAliases + vbNewLine + strReligion + vbNewLine + strAlignment + vbNewLine + strCreed + vbNewLine + strOccupation + vbNewLine + strIncome + vbNewLine + strMarital + vbNewLine + strPlayer + vbNewLine + strSheetFooter + vbNewLine + strLongForm + vbNewLine + strFooter
        'Open Output window
        Dim OutPut As New frmOutput(strWikiString, m_CharacterSheet.Full_Name)
        If Not Application.OpenForms().OfType(Of frmOutput).Any Then OutPut.Show()


    End Sub



    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles chkDilemeter.CheckedChanged
        If chkDilemeter.Checked = True Then
            txtDilemeter.Enabled = True
        Else
            txtDilemeter.Enabled = False
        End If
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Application.Exit()

    End Sub

    Private Sub HtmlRichTextBox1_Click(sender As Object, e As EventArgs) Handles HtmlRichTextBox1.Click
        Clipboard.SetText(HtmlRichTextBox1.GetHTML(True, False))
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbImages.SelectedIndexChanged
        'Show current image in picturebox

        Dim imagefilename As String = cmbImages.SelectedItem
        Dim imgFileName As String
        'Set up Filename
        imgFileName = m_CharacterSheet.Full_Name
        If imgFileName = "" Then imgFileName = m_CharacterSheet.Name
        imgFileName = Replace(imgFileName, " ", "")
        imgFileName = Regex.Replace(imgFileName, "[^A-Za-z0-9_. ]+", "")
        imgFileName = Regex.Replace(imgFileName, "[^A-Za-z0-9_. ]+", "")
        Dim strFull = WORKDIR + "\" + imgFileName + "\" + imagefilename

        PictureBox1.ImageLocation = strFull
        m_CharacterSheet.Picture = imagefilename
        PropertyGrid1.Refresh()
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles btnImagePath.Click
        'Configure output folder
        FolderBrowserDialog1.SelectedPath = WORKDIR

        FolderBrowserDialog1.ShowNewFolderButton = True
        FolderBrowserDialog1.ShowDialog()
        WORKDIR = FolderBrowserDialog1.SelectedPath
        txtImagePath.Text = FolderBrowserDialog1.SelectedPath

    End Sub

    Private Sub ParseTable_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        'Save Settings on Close
        My.Settings.Height = Me.Height
        My.Settings.Width = Me.Width
        My.Settings.Ontop = chkOnTop.Checked
        My.Settings.Debug = chkDebug.Checked
        My.Settings.UseDilemeter = chkDilemeter.Checked
        My.Settings.Dilemeter = txtDilemeter.Text
        My.Settings.WorkingDir = WORKDIR
        My.Settings.Save()

    End Sub
    Private Sub StatusStrip1_Resize(sender As Object, e As EventArgs) Handles StatusStrip1.Resize
        ToolStripProgressBar1.Width = StatusStrip1.Width - 200
    End Sub


    Private Sub Button1_Click_2(sender As Object, e As EventArgs) Handles Button1.Click
        If Not Application.OpenForms().OfType(Of frmMessage).Any Then
            Dim strtbtext As String = "Instructions For Using the BDI Wiki Parser

1. Download the character sheet from Google Docs by navigating to File. Select Download and save the sheet as an RTF (Rich Text Format). It must be saved as an RTF file to work properly. You will want to be sure that the player didn't change the formatting of the sheet, or move around any of the fields. 

2. Select ""Load"" to add a character sheet to the parser. It will take a few seconds to load properly, so just wait for it. 

3. Once the sheet is loaded, check to make sure the Use Delimiter option is selected. (It must be on for sheets that have a colon in the Biography, Personality, Description, Skills and Inventory sections of the sheet. Sheets made with an old copy of the blank template will not have this punctuation mark.)

4. Preview the sheet to make certain it propagated properly. 

5. Select which picture to use from the drop down menu. You can find this pic file in the folder that was created when you loaded the sheet into the parser. The folder will be located in the same directory you unzipped the app to.

6. Once you've selected the pic, you can generate the wiki markup by selecting 'Generate'. 

7. A new window will pop up with the generated wiki markup version of the sheet. You will need to add markup to link to locations or other characters, as well as for lists and bold formatting in the latter sections of the sheet. The parser will not add this markup automatically. 

8. Create the sheet on the wiki by logging in and searching for the name of the character. In the blank field, paste the generated markup. 

9. Make whatever necessary edits to the sheet and save it. 

10. Upload the picture file to the wiki. Make certain it is the same file you selected in the sheet. Once the picture is uploaded, it will take a few minutes for the wiki to propagate it into the sheet. (You can avoid this wait time by uploading the picture file before the sheet.)

Tada! You have successfully parsed a sheet from Google Docs format into the MediaWiki markup the BDI wiki uses. "
            Dim strTitle As String = "Help/About"
            Dim Message As New frmMessage(strtbtext, strtitle)
            Message.Show()
        End If
    End Sub

    Private Sub Label8_Click(sender As Object, e As EventArgs) Handles Label8.Click
        Dim m As DialogResult = MessageBox.Show("Choose whether or not to use a delimiter." + vbNewLine + vbNewLine + "Older sheets do not use one. Newer sheets use : by default. If you see fields beginning in an errant : character, toggle this on and reload the sheet." + vbNewLine + vbNewLine + "Please note this only effects the long-form fields from Biography through Goals.", "Quick help: Delimiter", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
    End Sub

End Class
