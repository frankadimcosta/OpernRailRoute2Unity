namespace OpernRailRoute2Unity
{
    public partial class Form1 : Form
    {


        private void btnTdb_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleziona il database dei binari";
                dialog.Filter = "Database Open Rails (*.tdb)|*.tdb|Tutti i file (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtTdb.Text = dialog.FileName;
                    Log("TDB selezionato: " + dialog.FileName);
                }
            }
        }

        private void btnTsection_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleziona tsection.dat";
                dialog.Filter = "TSection DAT (tsection.dat)|tsection.dat|Tutti i file (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtTsection.Text = dialog.FileName;
                    Log("TSection selezionato: " + dialog.FileName);
                }
            }
        }

        private void btnExport_Clic_LOGk(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                string.IsNullOrWhiteSpace(txtTsection.Text))
            {
                Log("Seleziona prima entrambi i file.");
                return;
            }

            Log("File selezionati. Pronto per la lettura del database.");



        }


        private void btnExport_Click_OLD2(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                var database = new Orts.Formats.Msts.TrackDatabaseFile(
                    txtTdb.Text);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];

                    if (node == null)
                        continue;

                    valid++;

                    if (node.TrVectorNode != null)
                        vector++;
                    else if (node.TrJunctionNode != null)
                        junction++;
                    else if (node.TrEndNode)
                        end++;
                }

                Log("Lettura TDB completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura del TDB:");
                Log(ex.ToString());
            }
        }


        private void btnExport_Click_OLD3(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                var database = new Orts.Formats.Msts.TrackDatabaseFile(
                    txtTdb.Text);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }


                if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
    !System.IO.File.Exists(txtTsection.Text))
                {
                    Log("Errore: seleziona un file TSECTION.DAT esistente.");
                    return;
                }

                Log("Apertura di TSECTION.DAT...");

                var tsection = new Orts.Formats.Msts.TrackSectionsFile(
                    txtTsection.Text);

                Log("TSECTION.DAT caricato.");


                nodes = database.TrackDB.TrackNodes;

                foreach (var node in nodes)
                {
                    if (node == null || node.TrJunctionNode == null)
                        continue;

                    uint shapeIndex = node.TrJunctionNode.ShapeIndex;

                    try
                    {
                        var shape = tsection.TrackShapes.Get(shapeIndex);

                        int mainPin = (int)node.Inpins + (int)shape.MainRoute;

                        int sidingPin = shape.MainRoute == 0
                            ? (int)node.Inpins + 1
                            : (int)node.Inpins;

                        Log(
                            "Scambio " + node.Index +
                            " | MainRoute=" + shape.MainRoute +
                            " | Pin principale=" + mainPin +
                            " | Pin secondario=" + sidingPin);
                    }
                    catch (System.IO.InvalidDataException ex)
                    {
                        Log(
                            "Impossibile leggere la forma dello scambio " +
                            node.Index + ": " + ex.Message);
                    }
                }

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;

                string outputPath = System.IO.Path.Combine(                    System.IO.Path.GetDirectoryName(                        System.IO.Path.GetFullPath(txtTdb.Text)),                    "track_points.txt");

                using (var writer = new System.IO.StreamWriter(
                    outputPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections = node.TrVectorNode.TrVectorSections;

                            if (sections == null)
                                continue;

                            foreach (var section in sections)
                            {
                                if (section == null)
                                    continue;

                                double x = section.TileX * 2048.0 + section.X;
                                double y = section.Y;
                                double z = section.TileZ * 2048.0 + section.Z;

                                writer.WriteLine(
                                    string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0:F3} {1:F3} {2:F3}",
                                        x, y, z));

                                points++;
                            }

                            // Riga vuota per separare i nodi vettoriali.
                            writer.WriteLine();
                        }
                        else if (node.TrJunctionNode != null)
                        {
                            junction++;
                        }
                        else if (node.TrEndNode)
                        {
                            end++;
                        }
                    }
                }

                Log("Lettura TDB completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Punti esportati: " + points);
                Log("File creato: " + outputPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione del TDB:");
                Log(ex.ToString());
            }
        }



        private void btnExport_Click_OLD4(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
                !System.IO.File.Exists(txtTsection.Text))
            {
                Log("Errore: seleziona un file TSECTION.DAT esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                var database = new Orts.Formats.Msts.TrackDatabaseFile(
                    txtTdb.Text);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

               /* Log("Apertura di TSECTION.DAT...");

                var tsection = new Orts.Formats.Msts.TrackSectionsFile(
                    txtTsection.Text);

                if (tsection.TrackShapes == null)
                {
                    Log("Errore: TrackShapes non è stato caricato.");
                    return;
                }

                Log("TSECTION.DAT caricato correttamente.");*/



                Log("Apertura di TSECTION.DAT standard...");

                string routeTsectionPath = txtTsection.Text;

                string globalTsectionPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.IO.Path.GetFullPath(routeTsectionPath)),
                    "..",
                    "..",
                    "GLOBAL",
                    "TSECTION.DAT");

                globalTsectionPath = System.IO.Path.GetFullPath(globalTsectionPath);

                if (!System.IO.File.Exists(globalTsectionPath))
                {
                    Log("Errore: TSECTION.DAT standard non trovato: " +
                        globalTsectionPath);
                    return;
                }

                var tsection = new Orts.Formats.Msts.TrackSectionsFile(
                    globalTsectionPath);

                Log("TSECTION.DAT standard caricato.");

                Log("Caricamento delle definizioni della route...");

                tsection.AddRouteTSectionDatFile(routeTsectionPath);

                Log("Definizioni della route caricate.");


                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;
                int pins = 0;
                int invalidLinks = 0;
                int shapeErrors = 0;

                string outputPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.IO.Path.GetFullPath(txtTdb.Text)),
                    "track_points.txt");

                using (var writer = new System.IO.StreamWriter(
                    outputPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        // Controlla i collegamenti TrPins.
                        if (node.TrPins != null)
                        {
                            foreach (var pin in node.TrPins)
                            {
                                if (pin == null)
                                    continue;

                                pins++;

                                if (pin.Link <= 0 ||
                                    pin.Link >= nodes.Length ||
                                    nodes[pin.Link] == null)
                                {
                                    invalidLinks++;

                                    Log(
                                        "Collegamento non valido: nodo " +
                                        i + ", Link=" + pin.Link);
                                }
                            }
                        }

                        // Nodi vettoriali: esporta i punti delle sezioni.
                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections =
                                node.TrVectorNode.TrVectorSections;

                            if (sections == null)
                                continue;

                            foreach (var section in sections)
                            {
                                if (section == null)
                                    continue;

                                double x =
                                    section.TileX * 2048.0 + section.X;

                                double y = section.Y;

                                double z =
                                    section.TileZ * 2048.0 + section.Z;

                                writer.WriteLine(
                                    string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0:F3} {1:F3} {2:F3}",
                                        x, y, z));

                                points++;
                            }

                            writer.WriteLine();
                        }
                        // Nodi di giunzione: verifica la forma in TSECTION.
                        else if (node.TrJunctionNode != null)
                        {
                            junction++;

                            uint shapeIndex =
                                node.TrJunctionNode.ShapeIndex;

                            try
                            {
                                var shape =
                                    tsection.TrackShapes.Get(shapeIndex);

                                int mainPin =
                                    (int)node.Inpins +
                                    (int)shape.MainRoute;

                                int sidingPin =
                                    shape.MainRoute == 0
                                        ? (int)node.Inpins + 1
                                        : (int)node.Inpins;

                                Log(
                                    "Scambio nodo " + i +
                                    " | ShapeIndex=" + shapeIndex +
                                    " | MainRoute=" + shape.MainRoute +
                                    " | Pin principale=" + mainPin +
                                    " | Pin secondario=" + sidingPin);

                                if (node.TrPins == null ||
                                    mainPin < 0 ||
                                    mainPin >= node.TrPins.Length ||
                                    sidingPin < 0 ||
                                    sidingPin >= node.TrPins.Length)
                                {
                                    Log(
                                        "Attenzione: indici dei pin fuori " +
                                        "dai limiti per il nodo " + i);
                                }
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Errore nella forma dello scambio " +
                                    shapeIndex + ": " + ex.Message);
                            }
                        }
                        else if (node.TrEndNode)
                        {
                            end++;
                        }
                    }
                }

                Log("Esportazione diagnostica completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Collegamenti TrPins esaminati: " + pins);
                Log("Collegamenti non validi: " + invalidLinks);
                Log("Errori nelle forme degli scambi: " + shapeErrors);
                Log("Punti esportati: " + points);
                Log("File creato: " + outputPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione:");
                Log(ex.ToString());
            }
        }


        private void btnExport_Click_OLD5(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
                !System.IO.File.Exists(txtTsection.Text))
            {
                Log("Errore: seleziona un file TSECTION.DAT esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                var database =
                    new Orts.Formats.Msts.TrackDatabaseFile(txtTdb.Text);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

                // Carica prima il TSECTION.DAT globale.
                Log("Apertura di TSECTION.DAT standard...");

                string routeTsectionPath = txtTsection.Text;

                string globalTsectionPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.IO.Path.GetFullPath(routeTsectionPath)),
                    "..",
                    "..",
                    "GLOBAL",
                    "TSECTION.DAT");

                globalTsectionPath =
                    System.IO.Path.GetFullPath(globalTsectionPath);

                if (!System.IO.File.Exists(globalTsectionPath))
                {
                    Log("Errore: TSECTION.DAT standard non trovato: " +
                        globalTsectionPath);
                    return;
                }

                var tsection =
                    new Orts.Formats.Msts.TrackSectionsFile(
                        globalTsectionPath);

                Log("TSECTION.DAT standard caricato.");
                Log("Caricamento delle definizioni della route...");

                tsection.AddRouteTSectionDatFile(routeTsectionPath);

                Log("Definizioni della route caricate.");








                string directory =
                    System.IO.Path.GetDirectoryName(
                        System.IO.Path.GetFullPath(txtTdb.Text));

                string pointsPath =
                    System.IO.Path.Combine(directory, "track_points.txt");

                string topologyPath =
                    System.IO.Path.Combine(directory, "track_topology.csv");

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;
                int pins = 0;
                int invalidLinks = 0;
                int shapeErrors = 0;
                int topologyRows = 0;

                // File diagnostico delle coordinate.
                using (var writer = new System.IO.StreamWriter(
                    pointsPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                // File diagnostico della topologia.
                using (var topologyWriter = new System.IO.StreamWriter(
                    topologyPath,
                    false,
                    new System.Text.UTF8Encoding(true)))
                {
                    topologyWriter.WriteLine(
                        "Tipo;IndiceNodo;Pin;NodoCollegato;" +
                        "ShapeIndex;MainRoute;PinPrincipale;" +
                        "PinSecondario;NumeroSezioni");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        // Esamina tutti i collegamenti TrPins.
                        if (node.TrPins != null)
                        {
                            for (int p = 0; p < node.TrPins.Length; p++)
                            {
                                var pin = node.TrPins[p];

                                if (pin == null)
                                    continue;

                                pins++;

                                if (pin.Link <= 0 ||
                                    pin.Link >= nodes.Length ||
                                    nodes[pin.Link] == null)
                                {
                                    invalidLinks++;

                                    Log("Collegamento non valido: nodo " +
                                        i + ", pin " + p +
                                        ", Link=" + pin.Link);
                                }
                            }
                        }

                        // Nodo vettoriale.
                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections =
                                node.TrVectorNode.TrVectorSections;

                            int sectionCount =
                                sections == null ? 0 : sections.Length;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "VETTORE;{0};{1};{2};;;;;{3}",
                                            i,
                                            p,
                                            pin.Link,
                                            sectionCount));

                                    topologyRows++;
                                }
                            }

                            if (sections == null)
                                continue;

                            foreach (var section in sections)
                            {
                                if (section == null)
                                    continue;

                                double x =
                                    section.TileX * 2048.0 + section.X;

                                double y = section.Y;

                                double z =
                                    section.TileZ * 2048.0 + section.Z;

                                writer.WriteLine(
                                    string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0:F3} {1:F3} {2:F3}",
                                        x, y, z));

                                points++;
                            }

                            writer.WriteLine();
                        }
                        // Nodo di scambio.
                        else if (node.TrJunctionNode != null)
                        {
                            junction++;

                            uint shapeIndex =
                                node.TrJunctionNode.ShapeIndex;

                            try
                            {
                                var shape =
                                    tsection.TrackShapes.Get(shapeIndex);

                                int mainPin =
                                    (int)node.Inpins +
                                    (int)shape.MainRoute;

                                int secondaryPin =
                                    shape.MainRoute == 0
                                        ? (int)node.Inpins + 1
                                        : (int)node.Inpins;

                                Log(
                                    "Scambio nodo " + i +
                                    " | ShapeIndex=" + shapeIndex +
                                    " | MainRoute=" + shape.MainRoute +
                                    " | Pin principale=" + mainPin +
                                    " | Pin secondario=" + secondaryPin);

                                if (node.TrPins == null ||
                                    mainPin < 0 ||
                                    mainPin >= node.TrPins.Length ||
                                    secondaryPin < 0 ||
                                    secondaryPin >= node.TrPins.Length)
                                {
                                    Log(
                                        "Attenzione: indici dei pin fuori " +
                                        "dai limiti per il nodo " + i);
                                }

                                if (node.TrPins != null)
                                {
                                    for (int p = 0;
                                         p < node.TrPins.Length;
                                         p++)
                                    {
                                        var pin = node.TrPins[p];

                                        if (pin == null)
                                            continue;

                                        string type;

                                        if (p == 0)
                                            type = "INGRESSO";
                                        else if (p == mainPin)
                                            type = "PRINCIPALE";
                                        else if (p == secondaryPin)
                                            type = "SECONDARIO";
                                        else
                                            type = "ALTRO";

                                        topologyWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "{0};{1};{2};{3};{4};{5};{6};{7};",
                                                type,
                                                i,
                                                p,
                                                pin.Link,
                                                shapeIndex,
                                                shape.MainRoute,
                                                mainPin,
                                                secondaryPin));

                                        topologyRows++;
                                    }
                                }
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Errore nella forma dello scambio " +
                                    shapeIndex + ": " + ex.Message);

                                topologyWriter.WriteLine(
                                    string.Format(
                                        "ERRORE_SHAPE;{0};;;;;;;",
                                        i));

                                topologyRows++;
                            }
                        }
                        // Nodo terminale.
                        else if (node.TrEndNode)
                        {
                            end++;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            "TERMINALE;{0};{1};{2};;;;;",
                                            i,
                                            p,
                                            pin.Link));

                                    topologyRows++;
                                }
                            }
                        }
                    }
                }

                Log("Esportazione diagnostica completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Collegamenti TrPins esaminati: " + pins);
                Log("Collegamenti non validi: " + invalidLinks);
                Log("Errori nelle forme degli scambi: " + shapeErrors);
                Log("Punti esportati: " + points);
                Log("Righe della topologia: " + topologyRows);
                Log("File coordinate: " + pointsPath);
                Log("File topologia: " + topologyPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione:");
                Log(ex.ToString());
            }
        }



        private void btnExport_Click_OLD6(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
                !System.IO.File.Exists(txtTsection.Text))
            {
                Log("Errore: seleziona un file TSECTION.DAT esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                string tdbFullPath =
                    System.IO.Path.GetFullPath(txtTdb.Text);

                var database =
                    new Orts.Formats.Msts.TrackDatabaseFile(tdbFullPath);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

                // ---------------------------------------------------------
                // Caricamento del TSECTION.DAT globale e di quello della route
                // ---------------------------------------------------------

                Log("Apertura di TSECTION.DAT standard...");

                string routeTsectionPath =
    System.IO.Path.GetFullPath(txtTsection.Text);

                string routeDirectory =
                    System.IO.Path.GetDirectoryName(routeTsectionPath)
                    ?? string.Empty;

                if (routeDirectory.Length == 0)
                {
                    Log("Errore: impossibile determinare la cartella della route.");
                    return;
                }

                string globalTsectionPath =
                    System.IO.Path.GetFullPath(
                        System.IO.Path.Combine(
                            routeDirectory,
                            "..",
                            "..",
                            "GLOBAL",
                            "TSECTION.DAT"));

                if (!System.IO.File.Exists(globalTsectionPath))
                {
                    Log("Errore: TSECTION.DAT standard non trovato: " +
                        globalTsectionPath);
                    return;
                }

                var tsection =
                    new Orts.Formats.Msts.TrackSectionsFile(
                        globalTsectionPath);

                Log("TSECTION.DAT standard caricato.");
                Log("Caricamento delle definizioni della route...");

                tsection.AddRouteTSectionDatFile(routeTsectionPath);

                Log("Definizioni della route caricate.");

                // ---------------------------------------------------------
                // Percorsi dei file di output
                // ---------------------------------------------------------

                string directory =
                    System.IO.Path.GetDirectoryName(tdbFullPath);

                if (string.IsNullOrEmpty(directory))
                {
                    Log("Errore: impossibile determinare la cartella del TDB.");
                    return;
                }

                string pointsPath =
                    System.IO.Path.Combine(directory, "track_points.txt");

                string topologyPath =
                    System.IO.Path.Combine(directory, "track_topology.csv");

                // ---------------------------------------------------------
                // Contatori
                // ---------------------------------------------------------

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;
                int pins = 0;
                int invalidLinks = 0;
                int shapeErrors = 0;
                int topologyRows = 0;

                // ---------------------------------------------------------
                // Esportazione diagnostica
                // ---------------------------------------------------------

                using (var writer = new System.IO.StreamWriter(
                    pointsPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                using (var topologyWriter = new System.IO.StreamWriter(
                    topologyPath,
                    false,
                    new System.Text.UTF8Encoding(true)))
                {
                    topologyWriter.WriteLine(
                        "Tipo;IndiceNodo;Pin;NodoCollegato;" +
                        "ShapeIndex;MainRoute;PinPrincipale;" +
                        "PinSecondario;NumeroSezioni");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        // -------------------------------------------------
                        // Controllo dei collegamenti TrPins
                        // -------------------------------------------------

                        if (node.TrPins != null)
                        {
                            for (int p = 0; p < node.TrPins.Length; p++)
                            {
                                var pin = node.TrPins[p];

                                if (pin == null)
                                    continue;

                                pins++;

                                // Link può essere long.
                                // Verifichiamo il valore prima del cast.
                                long link = pin.Link;

                                if (link <= 0 ||
                                    link >= nodes.Length ||
                                    nodes[checked((int)link)] == null)
                                {
                                    invalidLinks++;

                                    Log(
                                        "Collegamento non valido: nodo " +
                                        i + ", pin " + p +
                                        ", Link=" + link);
                                }
                            }
                        }

                        // -------------------------------------------------
                        // Nodo vettoriale
                        // -------------------------------------------------

                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections =
                                node.TrVectorNode.TrVectorSections;

                            int sectionCount =
                                sections == null ? 0 : sections.Length;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "VETTORE;{0};{1};{2};;;;;{3}",
                                            i,
                                            p,
                                            pin.Link,
                                            sectionCount));

                                    topologyRows++;
                                }
                            }

                            if (sections == null)
                                continue;

                            foreach (var section in sections)
                            {
                                if (section == null)
                                    continue;

                                double x =
                                    section.TileX * 2048.0 + section.X;

                                double y = section.Y;

                                double z =
                                    section.TileZ * 2048.0 + section.Z;

                                writer.WriteLine(
                                    string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0:F3} {1:F3} {2:F3}",
                                        x, y, z));

                                points++;
                            }

                            writer.WriteLine();
                        }

                        // -------------------------------------------------
                        // Nodo di scambio
                        // -------------------------------------------------

                        else if (node.TrJunctionNode != null)
                        {
                            junction++;

                            uint shapeIndex =
                                node.TrJunctionNode.ShapeIndex;

                            try
                            {
                                // Conversione esplicita da uint a int.
                                //int shapeIndexInt =                                    checked((int)shapeIndex);

                                var shape =
                                    tsection.TrackShapes.Get(shapeIndex);

                                int inpins =
                                    checked((int)node.Inpins);

                                int mainRoute =
                                    checked((int)shape.MainRoute);

                                int mainPin =
                                    inpins + mainRoute;

                                int secondaryPin =
                                    mainRoute == 0
                                        ? inpins + 1
                                        : inpins;

                                Log(
                                    "Scambio nodo " + i +
                                    " | ShapeIndex=" + shapeIndex +
                                    " | MainRoute=" + mainRoute +
                                    " | Pin principale=" + mainPin +
                                    " | Pin secondario=" + secondaryPin);

                                if (node.TrPins == null ||
                                    mainPin < 0 ||
                                    mainPin >= node.TrPins.Length ||
                                    secondaryPin < 0 ||
                                    secondaryPin >= node.TrPins.Length)
                                {
                                    Log(
                                        "Attenzione: indici dei pin fuori " +
                                        "dai limiti per il nodo " + i);
                                }

                                if (node.TrPins != null)
                                {
                                    for (int p = 0;
                                         p < node.TrPins.Length;
                                         p++)
                                    {
                                        var pin = node.TrPins[p];

                                        if (pin == null)
                                            continue;

                                        string type;

                                        if (p == 0)
                                            type = "INGRESSO";
                                        else if (p == mainPin)
                                            type = "PRINCIPALE";
                                        else if (p == secondaryPin)
                                            type = "SECONDARIO";
                                        else
                                            type = "ALTRO";

                                        topologyWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "{0};{1};{2};{3};{4};{5};{6};{7};",
                                                type,
                                                i,
                                                p,
                                                pin.Link,
                                                shapeIndex,
                                                mainRoute,
                                                mainPin,
                                                secondaryPin));

                                        topologyRows++;
                                    }
                                }
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Errore nella forma dello scambio " +
                                    shapeIndex + ": " + ex.Message);

                                topologyWriter.WriteLine(
                                    string.Format(
                                        "ERRORE_SHAPE;{0};;;;;;;",
                                        i));

                                topologyRows++;
                            }
                            catch (System.OverflowException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Indice fuori intervallo nello scambio " +
                                    i + ": " + ex.Message);
                            }
                        }

                        // -------------------------------------------------
                        // Nodo terminale
                        // -------------------------------------------------

                        else if (node.TrEndNode)
                        {
                            end++;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "TERMINALE;{0};{1};{2};;;;;",
                                            i,
                                            p,
                                            pin.Link));

                                    topologyRows++;
                                }
                            }
                        }
                    }
                }

                // ---------------------------------------------------------
                // Riepilogo
                // ---------------------------------------------------------

                Log("Esportazione diagnostica completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Collegamenti TrPins esaminati: " + pins);
                Log("Collegamenti non validi: " + invalidLinks);
                Log("Errori nelle forme degli scambi: " + shapeErrors);
                Log("Punti esportati: " + points);
                Log("Righe della topologia: " + topologyRows);
                Log("File coordinate: " + pointsPath);
                Log("File topologia: " + topologyPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione:");
                Log(ex.ToString());
            }
        }



        private void btnExport_Click_OK(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
                !System.IO.File.Exists(txtTsection.Text))
            {
                Log("Errore: seleziona un file TSECTION.DAT esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                string tdbFullPath =
                    System.IO.Path.GetFullPath(txtTdb.Text);

                var database =
                    new Orts.Formats.Msts.TrackDatabaseFile(tdbFullPath);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

                // Caricamento del TSECTION.DAT globale e della route.

                Log("Apertura di TSECTION.DAT standard...");

                string routeTsectionPath =
                    System.IO.Path.GetFullPath(txtTsection.Text);

                string routeDirectory =
                    System.IO.Path.GetDirectoryName(routeTsectionPath)
                    ?? string.Empty;

                if (routeDirectory.Length == 0)
                {
                    Log("Errore: impossibile determinare la cartella della route.");
                    return;
                }

                string globalTsectionPath =
                    System.IO.Path.GetFullPath(
                        System.IO.Path.Combine(
                            routeDirectory,
                            "..",
                            "..",
                            "GLOBAL",
                            "TSECTION.DAT"));

                if (!System.IO.File.Exists(globalTsectionPath))
                {
                    Log("Errore: TSECTION.DAT standard non trovato: " +
                        globalTsectionPath);
                    return;
                }

                var tsection =
                    new Orts.Formats.Msts.TrackSectionsFile(
                        globalTsectionPath);

                Log("TSECTION.DAT standard caricato.");
                Log("Caricamento delle definizioni della route...");

                tsection.AddRouteTSectionDatFile(routeTsectionPath);

                Log("Definizioni della route caricate.");

                // Percorsi dei file di output.

                string directory =
                    System.IO.Path.GetDirectoryName(tdbFullPath);

                if (string.IsNullOrEmpty(directory))
                {
                    Log("Errore: impossibile determinare la cartella del TDB.");
                    return;
                }

                string pointsPath =
                    System.IO.Path.Combine(directory, "track_points.txt");

                string topologyPath =
                    System.IO.Path.Combine(directory, "track_topology.csv");

                string diagnosticPath =
                    System.IO.Path.Combine(
                        directory, "track_vectors_diagnostic.txt");

                // Contatori.

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;
                int pins = 0;
                int invalidLinks = 0;
                int shapeErrors = 0;
                int topologyRows = 0;
                int singleSectionVectors = 0;

                // Esportazione diagnostica.

                using (var writer = new System.IO.StreamWriter(
                    pointsPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                using (var topologyWriter = new System.IO.StreamWriter(
                    topologyPath,
                    false,
                    new System.Text.UTF8Encoding(true)))
                using (var diagnosticWriter = new System.IO.StreamWriter(
                    diagnosticPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                {
                    topologyWriter.WriteLine(
                        "Tipo;IndiceNodo;Pin;NodoCollegato;" +
                        "ShapeIndex;MainRoute;PinPrincipale;" +
                        "PinSecondario;NumeroSezioni");

                    diagnosticWriter.WriteLine(
                        "IndiceArray;IndiceNodo;NumeroSezioni;" +
                        "X;Y;Z;Pin;NodoCollegato");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        // Controllo dei collegamenti TrPins.

                        if (node.TrPins != null)
                        {
                            for (int p = 0; p < node.TrPins.Length; p++)
                            {
                                var pin = node.TrPins[p];

                                if (pin == null)
                                    continue;

                                pins++;

                                long link = pin.Link;

                                if (link <= 0 ||
                                    link >= nodes.Length ||
                                    nodes[checked((int)link)] == null)
                                {
                                    invalidLinks++;

                                    Log(
                                        "Collegamento non valido: nodo " +
                                        i + ", pin " + p +
                                        ", Link=" + link);
                                }
                            }
                        }

                        // Nodo vettoriale.

                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections =
                                node.TrVectorNode.TrVectorSections;

                            int sectionCount =
                                sections == null ? 0 : sections.Length;

                            // Nuova diagnostica: vettori con una sola sezione.

                            if (sectionCount == 1 && sections[0] != null)
                            {
                                singleSectionVectors++;

                                var section = sections[0];

                                double x =
                                    section.TileX * 2048.0 + section.X;

                                double y = section.Y;

                                double z =
                                    section.TileZ * 2048.0 + section.Z;

                                if (node.TrPins != null &&
                                    node.TrPins.Length > 0)
                                {
                                    for (int p = 0;
                                         p < node.TrPins.Length;
                                         p++)
                                    {
                                        var pin = node.TrPins[p];

                                        if (pin == null)
                                            continue;

                                        diagnosticWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "{0};{1};{2};{3:F3};{4:F3};{5:F3};{6};{7}",
                                                i,
                                                node.Index,
                                                sectionCount,
                                                x, y, z,
                                                p,
                                                pin.Link));
                                    }
                                }
                                else
                                {
                                    diagnosticWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "{0};{1};{2};{3:F3};{4:F3};{5:F3};-1;-1",
                                            i,
                                            node.Index,
                                            sectionCount,
                                            x, y, z));
                                }
                            }

                            // Esportazione originale delle coordinate.

                            if (sections == null)
                                continue;

                            foreach (var section in sections)
                            {
                                if (section == null)
                                    continue;

                                double x =
                                    section.TileX * 2048.0 + section.X;

                                double y = section.Y;

                                double z =
                                    section.TileZ * 2048.0 + section.Z;

                                writer.WriteLine(
                                    string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0:F3} {1:F3} {2:F3}",
                                        x, y, z));

                                points++;
                            }

                            writer.WriteLine();
                        }

                        // Nodo di scambio.

                        else if (node.TrJunctionNode != null)
                        {
                            junction++;

                            uint shapeIndex =
                                node.TrJunctionNode.ShapeIndex;

                            try
                            {
                                var shape =
                                    tsection.TrackShapes.Get(shapeIndex);

                                int inpins =
                                    checked((int)node.Inpins);

                                int mainRoute =
                                    checked((int)shape.MainRoute);

                                int mainPin =
                                    inpins + mainRoute;

                                int secondaryPin =
                                    mainRoute == 0
                                        ? inpins + 1
                                        : inpins;

                                Log(
                                    "Scambio nodo " + i +
                                    " | ShapeIndex=" + shapeIndex +
                                    " | MainRoute=" + mainRoute +
                                    " | Pin principale=" + mainPin +
                                    " | Pin secondario=" + secondaryPin);

                                if (node.TrPins == null ||
                                    mainPin < 0 ||
                                    mainPin >= node.TrPins.Length ||
                                    secondaryPin < 0 ||
                                    secondaryPin >= node.TrPins.Length)
                                {
                                    Log(
                                        "Attenzione: indici dei pin fuori " +
                                        "dai limiti per il nodo " + i);
                                }

                                if (node.TrPins != null)
                                {
                                    for (int p = 0;
                                         p < node.TrPins.Length;
                                         p++)
                                    {
                                        var pin = node.TrPins[p];

                                        if (pin == null)
                                            continue;

                                        string type;

                                        if (p == 0)
                                            type = "INGRESSO";
                                        else if (p == mainPin)
                                            type = "PRINCIPALE";
                                        else if (p == secondaryPin)
                                            type = "SECONDARIO";
                                        else
                                            type = "ALTRO";

                                        topologyWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "{0};{1};{2};{3};{4};{5};{6};{7};",
                                                type,
                                                i,
                                                p,
                                                pin.Link,
                                                shapeIndex,
                                                mainRoute,
                                                mainPin,
                                                secondaryPin));

                                        topologyRows++;
                                    }
                                }
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Errore nella forma dello scambio " +
                                    shapeIndex + ": " + ex.Message);

                                topologyWriter.WriteLine(
                                    string.Format(
                                        "ERRORE_SHAPE;{0};;;;;;;",
                                        i));

                                topologyRows++;
                            }
                            catch (System.OverflowException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Indice fuori intervallo nello scambio " +
                                    i + ": " + ex.Message);
                            }
                        }

                        // Nodo terminale.

                        else if (node.TrEndNode)
                        {
                            end++;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "TERMINALE;{0};{1};{2};;;;;",
                                            i,
                                            p,
                                            pin.Link));

                                    topologyRows++;
                                }
                            }
                        }
                    }
                }

                // Riepilogo.

                Log("Esportazione diagnostica completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Collegamenti TrPins esaminati: " + pins);
                Log("Collegamenti non validi: " + invalidLinks);
                Log("Errori nelle forme degli scambi: " + shapeErrors);
                Log("Punti esportati: " + points);
                Log("Vettori con una sola sezione: " +
                    singleSectionVectors);
                Log("Righe della topologia: " + topologyRows);
                Log("File coordinate: " + pointsPath);
                Log("File topologia: " + topologyPath);
                Log("File diagnostica vettori: " + diagnosticPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione:");
                Log(ex.ToString());
            }
        }


        private void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTdb.Text) ||
                !System.IO.File.Exists(txtTdb.Text))
            {
                Log("Errore: seleziona un file TDB esistente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTsection.Text) ||
                !System.IO.File.Exists(txtTsection.Text))
            {
                Log("Errore: seleziona un file TSECTION.DAT esistente.");
                return;
            }

            try
            {
                Log("Apertura del database TDB...");

                string tdbFullPath =
                    System.IO.Path.GetFullPath(txtTdb.Text);

                var database =
                    new Orts.Formats.Msts.TrackDatabaseFile(tdbFullPath);

                if (database.TrackDB == null)
                {
                    Log("Errore: TrackDB non è stato caricato.");
                    return;
                }

                var nodes = database.TrackDB.TrackNodes;

                if (nodes == null)
                {
                    Log("Errore: la tabella TrackNodes è nulla.");
                    return;
                }

                // ---------------------------------------------------------
                // Caricamento TSECTION.DAT
                // ---------------------------------------------------------

                Log("Apertura di TSECTION.DAT standard...");

                string routeTsectionPath =
                    System.IO.Path.GetFullPath(txtTsection.Text);

                string routeDirectory =
                    System.IO.Path.GetDirectoryName(routeTsectionPath)
                    ?? string.Empty;

                if (routeDirectory.Length == 0)
                {
                    Log("Errore: impossibile determinare la cartella della route.");
                    return;
                }

                string globalTsectionPath =
                    System.IO.Path.GetFullPath(
                        System.IO.Path.Combine(
                            routeDirectory,
                            "..",
                            "..",
                            "GLOBAL",
                            "TSECTION.DAT"));

                if (!System.IO.File.Exists(globalTsectionPath))
                {
                    Log("Errore: TSECTION.DAT standard non trovato: " +
                        globalTsectionPath);
                    return;
                }

                var tsection =
                    new Orts.Formats.Msts.TrackSectionsFile(
                        globalTsectionPath);

                Log("TSECTION.DAT standard caricato.");
                Log("Caricamento delle definizioni della route...");

                tsection.AddRouteTSectionDatFile(routeTsectionPath);

                Log("Definizioni della route caricate.");

                // ---------------------------------------------------------
                // Percorsi dei file di output
                // ---------------------------------------------------------

                string directory =
                    System.IO.Path.GetDirectoryName(tdbFullPath);

                if (string.IsNullOrEmpty(directory))
                {
                    Log("Errore: impossibile determinare la cartella del TDB.");
                    return;
                }

                string pointsPath =
                    System.IO.Path.Combine(directory, "track_points.txt");

                string topologyPath =
                    System.IO.Path.Combine(directory, "track_topology.csv");

                string switchDiagnosticPath =
                    System.IO.Path.Combine(
                        directory, "switch_geometry_diagnostic.txt");

                // ---------------------------------------------------------
                // Contatori esportazione
                // ---------------------------------------------------------

                int total = nodes.Length;
                int valid = 0;
                int vector = 0;
                int junction = 0;
                int end = 0;
                int points = 0;
                int pins = 0;
                int invalidLinks = 0;
                int shapeErrors = 0;
                int topologyRows = 0;

                // ---------------------------------------------------------
                // Esportazione coordinate e topologia
                // ---------------------------------------------------------

                using (var writer = new System.IO.StreamWriter(
                    pointsPath,
                    false,
                    new System.Text.UTF8Encoding(false)))
                using (var topologyWriter = new System.IO.StreamWriter(
                    topologyPath,
                    false,
                    new System.Text.UTF8Encoding(true)))
                {
                    topologyWriter.WriteLine(
                        "Tipo;IndiceNodo;Pin;NodoCollegato;" +
                        "ShapeIndex;MainRoute;PinPrincipale;" +
                        "PinSecondario;NumeroSezioni");

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];

                        if (node == null)
                            continue;

                        valid++;

                        // Controllo collegamenti.
                        if (node.TrPins != null)
                        {
                            for (int p = 0; p < node.TrPins.Length; p++)
                            {
                                var pin = node.TrPins[p];

                                if (pin == null)
                                    continue;

                                pins++;

                                long link = pin.Link;

                                if (link <= 0 ||
                                    link >= nodes.Length ||
                                    nodes[checked((int)link)] == null)
                                {
                                    invalidLinks++;

                                    Log(
                                        "Collegamento non valido: nodo " +
                                        i + ", pin " + p +
                                        ", Link=" + link);
                                }
                            }
                        }

                        // -------------------------------------------------
                        // Nodo vettoriale
                        // -------------------------------------------------

                        if (node.TrVectorNode != null)
                        {
                            vector++;

                            var sections =
                                node.TrVectorNode.TrVectorSections;

                            int sectionCount =
                                sections == null ? 0 : sections.Length;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "VETTORE;{0};{1};{2};;;;;{3}",
                                            i,
                                            p,
                                            pin.Link,
                                            sectionCount));

                                    topologyRows++;
                                }
                            }

                            if (sections != null)
                            {
                                foreach (var section in sections)
                                {
                                    if (section == null)
                                        continue;

                                    double x =
                                        section.TileX * 2048.0 + section.X;

                                    double y = section.Y;

                                    double z =
                                        section.TileZ * 2048.0 + section.Z;

                                    writer.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "{0:F3} {1:F3} {2:F3}",
                                            x, y, z));

                                    points++;
                                }
                            }

                            writer.WriteLine();
                        }

                        // -------------------------------------------------
                        // Nodo di scambio
                        // -------------------------------------------------

                        else if (node.TrJunctionNode != null)
                        {
                            junction++;

                            uint shapeIndex =
                                node.TrJunctionNode.ShapeIndex;

                            try
                            {
                                var shape =
                                    tsection.TrackShapes.Get(shapeIndex);

                                int inputPins =
                                    checked((int)node.Inpins);

                                int mainRoute =
                                    checked((int)shape.MainRoute);

                                int mainPin =
                                    inputPins + mainRoute;

                                int secondaryPin =
                                    mainRoute == 0
                                        ? inputPins + 1
                                        : inputPins;

                                Log(
                                    "Scambio nodo " + i +
                                    " | ShapeIndex=" + shapeIndex +
                                    " | MainRoute=" + mainRoute +
                                    " | Pin principale=" + mainPin +
                                    " | Pin secondario=" + secondaryPin);

                                if (node.TrPins != null)
                                {
                                    for (int p = 0;
                                         p < node.TrPins.Length;
                                         p++)
                                    {
                                        var pin = node.TrPins[p];

                                        if (pin == null)
                                            continue;

                                        string type;

                                        if (p == 0)
                                            type = "INGRESSO";
                                        else if (p == mainPin)
                                            type = "PRINCIPALE";
                                        else if (p == secondaryPin)
                                            type = "SECONDARIO";
                                        else
                                            type = "ALTRO";

                                        topologyWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "{0};{1};{2};{3};{4};{5};{6};{7};",
                                                type,
                                                i,
                                                p,
                                                pin.Link,
                                                shapeIndex,
                                                mainRoute,
                                                mainPin,
                                                secondaryPin));

                                        topologyRows++;
                                    }
                                }
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Errore nella forma dello scambio " +
                                    shapeIndex + ": " + ex.Message);

                                topologyWriter.WriteLine(
                                    string.Format(
                                        "ERRORE_SHAPE;{0};;;;;;;",
                                        i));

                                topologyRows++;
                            }
                            catch (System.OverflowException ex)
                            {
                                shapeErrors++;

                                Log(
                                    "Indice fuori intervallo nello scambio " +
                                    i + ": " + ex.Message);
                            }
                        }

                        // -------------------------------------------------
                        // Nodo terminale
                        // -------------------------------------------------

                        else if (node.TrEndNode)
                        {
                            end++;

                            if (node.TrPins != null)
                            {
                                for (int p = 0;
                                     p < node.TrPins.Length;
                                     p++)
                                {
                                    var pin = node.TrPins[p];

                                    if (pin == null)
                                        continue;

                                    topologyWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "TERMINALE;{0};{1};{2};;;;;",
                                            i,
                                            p,
                                            pin.Link));

                                    topologyRows++;
                                }
                            }
                        }
                    }
                }

                // ---------------------------------------------------------
                // DIAGNOSTICA GEOMETRICA DEGLI SCAMBI 161 E 175
                // Usa le classi effettive del parser Open Rails.
                // Non modifica i dati esportati sopra.
                // ---------------------------------------------------------

                int[] diagnosticSwitchIds = { 161, 175 };

                using (var diagnosticWriter =
                    new System.IO.StreamWriter(
                        switchDiagnosticPath,
                        false,
                        new System.Text.UTF8Encoding(false)))
                {
                    diagnosticWriter.WriteLine(
                        "DIAGNOSTICA GEOMETRIA E COLLEGAMENTI");

                    diagnosticWriter.WriteLine(
                        "Coordinate mondiali: Tile * 2048 + offset.");

                    foreach (int diagnosticSwitchId in diagnosticSwitchIds)
                    {
                        diagnosticWriter.WriteLine();
                        diagnosticWriter.WriteLine(
                            "==================================================");

                        diagnosticWriter.WriteLine(
                            "SCAMBIO " + diagnosticSwitchId);

                        if (diagnosticSwitchId < 0 ||
                            diagnosticSwitchId >= nodes.Length ||
                            nodes[diagnosticSwitchId] == null)
                        {
                            diagnosticWriter.WriteLine(
                                "Nodo non disponibile.");

                            continue;
                        }

                        var diagnosticJunction =
                            nodes[diagnosticSwitchId];

                        if (diagnosticJunction.TrJunctionNode == null)
                        {
                            diagnosticWriter.WriteLine(
                                "Il nodo non è un nodo di scambio.");

                            continue;
                        }

                        diagnosticWriter.WriteLine(
                            "Indice nodo: " + diagnosticJunction.Index);

                        diagnosticWriter.WriteLine(
                            "ShapeIndex: " +
                            diagnosticJunction.TrJunctionNode.ShapeIndex);

                        diagnosticWriter.WriteLine(
                            "InpIn: " + diagnosticJunction.Inpins);

                        diagnosticWriter.WriteLine(
                            "Outpins: " + diagnosticJunction.Outpins);


                        if (diagnosticJunction.UiD != null)
                        {
                            var uid = diagnosticJunction.UiD;

                            double switchX = uid.TileX * 2048.0 + uid.X;
                            double switchZ = uid.TileZ * 2048.0 + uid.Z;

                            diagnosticWriter.WriteLine(
                                string.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Posizione scambio: X={0:F3}; Y={1:F3}; Z={2:F3}",
                                    switchX,
                                    uid.Y,
                                    switchZ));
                        }


                        if (diagnosticJunction.TrPins == null)
                            continue;

                        for (int diagnosticPinIndex = 0;
                             diagnosticPinIndex <
                             diagnosticJunction.TrPins.Length;
                             diagnosticPinIndex++)
                        {
                            var diagnosticPin =
                                diagnosticJunction.TrPins[diagnosticPinIndex];

                            if (diagnosticPin == null)
                                continue;

                            long diagnosticLink = diagnosticPin.Link;

                            diagnosticWriter.WriteLine();
                            diagnosticWriter.WriteLine(
                                "PIN " + diagnosticPinIndex +
                                " -> nodo " + diagnosticLink);

                            if (diagnosticLink < 0 ||
                                diagnosticLink >= nodes.Length ||
                                nodes[checked((int)diagnosticLink)] == null)
                            {
                                diagnosticWriter.WriteLine(
                                    "Nodo collegato non disponibile.");

                                continue;
                            }

                            var diagnosticLinkedNode =
                                nodes[checked((int)diagnosticLink)];

                            diagnosticWriter.WriteLine(
                                "Indice nodo collegato: " +
                                diagnosticLinkedNode.Index);

                            // Il vettore si trova dentro TrackNode.TrVectorNode.
                            if (diagnosticLinkedNode.TrVectorNode != null)
                            {
                                var diagnosticSections =
                                    diagnosticLinkedNode.TrVectorNode.TrVectorSections;

                                int diagnosticSectionCount =
                                    diagnosticSections == null
                                        ? 0
                                        : diagnosticSections.Length;

                                diagnosticWriter.WriteLine(
                                    "Tipo: VETTORE");

                                diagnosticWriter.WriteLine(
                                    "Numero sezioni: " +
                                    diagnosticSectionCount);

                                if (diagnosticSections != null)
                                {
                                    for (int diagnosticSectionIndex = 0;
                                         diagnosticSectionIndex <
                                         diagnosticSections.Length;
                                         diagnosticSectionIndex++)
                                    {
                                        var diagnosticSection =
                                            diagnosticSections[diagnosticSectionIndex];

                                        if (diagnosticSection == null)
                                            continue;

                                        double diagnosticX =
                                            diagnosticSection.TileX * 2048.0 +
                                            diagnosticSection.X;

                                        double diagnosticZ =
                                            diagnosticSection.TileZ * 2048.0 +
                                            diagnosticSection.Z;

                                        diagnosticWriter.WriteLine(
                                            string.Format(
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                "Sezione {0}: X={1:F3}; Y={2:F3}; Z={3:F3}; TileX={4}; TileZ={5}; OffsetX={6:F3}; OffsetZ={7:F3}",
                                                diagnosticSectionIndex,
                                                diagnosticX,
                                                diagnosticSection.Y,
                                                diagnosticZ,
                                                diagnosticSection.TileX,
                                                diagnosticSection.TileZ,
                                                diagnosticSection.X,
                                                diagnosticSection.Z));
                                    }
                                }

                                if (diagnosticLinkedNode.TrPins != null)
                                {
                                    for (int diagnosticVectorPinIndex = 0;
                                         diagnosticVectorPinIndex <
                                         diagnosticLinkedNode.TrPins.Length;
                                         diagnosticVectorPinIndex++)
                                    {
                                        var diagnosticVectorPin =
                                            diagnosticLinkedNode.TrPins[
                                                diagnosticVectorPinIndex];

                                        if (diagnosticVectorPin == null)
                                            continue;

                                        diagnosticWriter.WriteLine(
                                            "Pin vettore " +
                                            diagnosticVectorPinIndex +
                                            " -> " + diagnosticVectorPin.Link);
                                    }
                                }
                            }
                            else if (diagnosticLinkedNode.TrJunctionNode != null)
                            {
                                diagnosticWriter.WriteLine(
                                    "Tipo: SCAMBIO");

                                diagnosticWriter.WriteLine(
                                    "ShapeIndex: " +
                                    diagnosticLinkedNode.TrJunctionNode.ShapeIndex);

                                if (diagnosticLinkedNode.UiD != null)
                                {
                                    var diagnosticUid =
                                        diagnosticLinkedNode.UiD;

                                    double diagnosticJunctionX =
                                        diagnosticUid.TileX * 2048.0 +
                                        diagnosticUid.X;

                                    double diagnosticJunctionZ =
                                        diagnosticUid.TileZ * 2048.0 +
                                        diagnosticUid.Z;

                                    diagnosticWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "Posizione UiD: X={0:F3}; Y={1:F3}; Z={2:F3}; TileX={3}; TileZ={4}; OffsetX={5:F3}; OffsetZ={6:F3}",
                                            diagnosticJunctionX,
                                            diagnosticUid.Y,
                                            diagnosticJunctionZ,
                                            diagnosticUid.TileX,
                                            diagnosticUid.TileZ,
                                            diagnosticUid.X,
                                            diagnosticUid.Z));
                                }
                            }
                            else if (diagnosticLinkedNode.TrEndNode)
                            {
                                diagnosticWriter.WriteLine(
                                    "Tipo: TERMINALE");

                                if (diagnosticLinkedNode.UiD != null)
                                {
                                    var diagnosticUid =
                                        diagnosticLinkedNode.UiD;

                                    diagnosticWriter.WriteLine(
                                        string.Format(
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            "UiD: TileX={0}; TileZ={1}; X={2:F3}; Y={3:F3}; Z={4:F3}",
                                            diagnosticUid.TileX,
                                            diagnosticUid.TileZ,
                                            diagnosticUid.X,
                                            diagnosticUid.Y,
                                            diagnosticUid.Z));
                                }
                            }
                            else
                            {
                                diagnosticWriter.WriteLine(
                                    "Tipo nodo non riconosciuto.");
                            }
                        }
                    }
                }

                // ---------------------------------------------------------
                // Riepilogo
                // ---------------------------------------------------------

                Log("Esportazione diagnostica completata.");
                Log("Elementi nell'array TrackNodes: " + total);
                Log("Nodi validi: " + valid);
                Log("Nodi vettoriali: " + vector);
                Log("Nodi di giunzione: " + junction);
                Log("Nodi terminali: " + end);
                Log("Collegamenti TrPins esaminati: " + pins);
                Log("Collegamenti non validi: " + invalidLinks);
                Log("Errori nelle forme degli scambi: " + shapeErrors);
                Log("Punti esportati: " + points);
                Log("Righe della topologia: " + topologyRows);
                Log("File coordinate: " + pointsPath);
                Log("File topologia: " + topologyPath);
                Log("File diagnostica scambi: " + switchDiagnosticPath);
            }
            catch (Exception ex)
            {
                Log("Errore durante la lettura o esportazione:");
                Log(ex.ToString());
            }
        }


        private void Log(string message)
        {
            txtLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        public Form1()
        {
            InitializeComponent();
            Log("OpenRailsTrackExporter avviato.");
            Log("Librerie Open Rails pronte per la verifica.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
