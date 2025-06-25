First test 1


1. Ziel des Projekts
Die Anwendung soll Nutzer:innen durch geführte Atemübungen begleiten. Die Atmung wird über das Mikrofon erkannt. Ein visuelles Feedback (ein animierter Bild) hilft dabei, das Atemtempo zu steuern. Am Ende der Übung erhält der Benutzer eine Bewertung (Score), die seine Leistung widerspiegelt.

2. Bisher umgesetzte Funktionen

2.1 Mikrofonintegration
Das Mikrofon des Geräts wird verwendet, um den aktuellen Lautstärkepegel des Benutzers zu messen.
Das Mikrofon startet automatisch zu Beginn der Übung.
Die Lautstärke wird in Echtzeit ausgewertet.

2.2 UI-Elemente
Atem-Bild(BreathingImage): Ein UI-Image, das sich basierend auf der Lautstärke und der Atemphase (Einatmen/Ausatmen) skaliert.
Textanzeigen:
instructionText: Zeigt die aktuelle Atemanweisung („Einatmen...“ / „Ausatmen...“).
feedbackText: Gibt am Ende der Übung Rückmeldung („Gut gemacht!“ oder „Versuch’s nochmal“).
timerText: Zeigt die verbleibende Zeit während der Übung.
scoreText: Zeigt den Endscore in Prozent.
Lautstärkeregler (Slider): Zeigt die erfasste Mikrofonlautstärke als Feedback.

2.3 Atemlogik
Die Atemübung besteht aus abwechselnden Phasen:
Einatmen (Inhale): 2 Sekunden
Ausatmen (Exhale): 2 Sekunden
Die Übungsdauer beträgt insgesamt 20 Sekunden.
Jede Phase wird als „erfolgreich“ gezählt, wenn ein Mindest-Lautstärkepegel erkannt wird.
Am Ende wird ein Score berechnet: (erfolgreiche Phasen / Gesamtphasen) × 100 %.

2.4 Steuerung über Buttons
Start-Button: Startet die Übung und aktiviert die relevanten UI-Elemente.
Pause-Button: Unterbricht die Übung temporär.
Stop-Button: Beendet die aktuelle Übung manuell.
Restart-Button: Startet die Übung neu.
UI-Logik:
Buttons erscheinen und verschwinden dynamisch je nach Übungsstatus (z. B. Start → unsichtbar nach Beginn).

3. Technische Umsetzung

3.1 Verwendete Technologie
Unity Engine (C#)
UI-System von Unity (Canvas, Text, Image, Slider)
Mikrofonzugriff über Microphone-API
Audioverarbeitung zur Berechnung des RMS-Volumens aus AudioSamples

3.2 Programmstruktur (Script BreathingManager.cs)
Initialisiert das Mikrofon und startet die Audioaufnahme.
Steuert die aktuelle Phase (Einatmen/Ausatmen).
Berechnet die Lautstärke des Benutzers live.
Animiert den Bild visuell in Abhängigkeit vom Atemverhalten.
Verwaltet die Sitzungsdauer und den Timer.
Bewertet die Leistung am Ende mit Feedback + Score.
