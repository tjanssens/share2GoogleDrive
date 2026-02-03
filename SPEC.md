# Share2GoogleDrive - Specificatie

## Overzicht

Share2GoogleDrive is een desktop applicatie die gebruikers toelaat om bestanden snel naar Google Drive te uploaden via het Windows context menu (rechtermuisklik) of een sneltoets.

## Functionele Vereisten

### 1. Context Menu Integratie

- **Functionaliteit**: Bij rechtsklikken op een bestand verschijnt een menu-item "Send 2 Drive"
- **Gedrag**:
  - Klikken op "Send 2 Drive" start de upload naar de geconfigureerde Google Drive map
  - Ondersteunt enkelvoudige en meervoudige bestandsselectie
  - Toont voortgangsindicatie tijdens upload
  - Geeft melding bij succesvolle upload of fout

### 2. Sneltoets Ondersteuning

- **Standaard sneltoets**: `Ctrl + G`
- **Gedrag**:
  - Werkt wanneer een bestand geselecteerd is in Windows Verkenner
  - Zelfde functionaliteit als context menu optie
  - Sneltoets is configureerbaar via instellingen

### 3. Instellingen

#### 3.1 Google Account (OAuth)
- OAuth 2.0 authenticatie met Google
- Mogelijkheid om in te loggen/uit te loggen
- Weergave van verbonden account (e-mail)
- Ondersteuning voor meerdere accounts (optioneel)
- Veilige opslag van tokens (encrypted)

#### 3.2 Doelmap op Google Drive
- Mapkiezer/browser voor Google Drive mappen
- Mogelijkheid om standaard doelmap in te stellen
- Optie om "Mijn Drive" root als standaard te gebruiken
- Mogelijkheid om nieuwe map aan te maken

#### 3.3 Sneltoets Configuratie
- Aanpasbare sneltoets combinatie
- Validatie om conflicten met systeemsneltoetsen te voorkomen
- Reset naar standaard optie

### 4. Gebruikersinterface

#### 4.1 Systeem Tray Icoon
- Applicatie draait in systeem tray (system tray)
- Rechtsklik menu met:
  - Instellingen openen
  - Upload geschiedenis bekijken
  - Afsluiten

#### 4.2 Instellingen Venster
- Tabbladen voor verschillende instellingscategorieën:
  - Account
  - Upload instellingen
  - Sneltoetsen
  - Algemeen (autostart, notificaties, etc.)

#### 4.3 Notificaties
- Windows toast notificaties voor:
  - Upload gestart
  - Upload voltooid (met link naar bestand)
  - Upload mislukt (met foutmelding)

## Technische Specificaties

### Platform
- **Primair**: Windows 10/11
- **Taal**: Python 3.10+ of Electron/Node.js
- **Packaging**: Installer (MSI/EXE) en portable versie

### Google Drive API
- Google Drive API v3
- OAuth 2.0 voor authenticatie
- Scopes:
  - `https://www.googleapis.com/auth/drive.file`
  - `https://www.googleapis.com/auth/drive.metadata.readonly`

### Context Menu Registratie
- Windows Registry modificatie voor shell integratie
- Registratie onder `HKEY_CLASSES_ROOT\*\shell\Send2Drive`

### Sneltoets Implementatie
- Globale hotkey registratie
- Background service voor hotkey listening

### Data Opslag
- Configuratie: JSON of SQLite
- Tokens: Windows Credential Manager of encrypted file
- Locatie: `%APPDATA%\Share2GoogleDrive\`

## Niet-Functionele Vereisten

### Performance
- Upload start binnen 2 seconden na actie
- Minimale CPU/geheugen gebruik in idle
- Ondersteuning voor bestanden tot 5GB

### Beveiliging
- Geen opslag van wachtwoorden
- Encrypted token opslag
- Secure OAuth flow (geen embedded browser)

### Betrouwbaarheid
- Automatische retry bij netwerk fouten (max 3 pogingen)
- Hervatbare uploads voor grote bestanden
- Logging voor troubleshooting

### Gebruikerservaring
- Installatie zonder admin rechten (user-level)
- Autostart optie
- Minimale configuratie vereist voor eerste gebruik

## Architectuur

```
share2GoogleDrive/
├── src/
│   ├── main.py                 # Applicatie entry point
│   ├── config/
│   │   ├── settings.py         # Instellingen beheer
│   │   └── constants.py        # Constanten en defaults
│   ├── auth/
│   │   ├── oauth.py            # Google OAuth implementatie
│   │   └── token_manager.py    # Token opslag en verversing
│   ├── drive/
│   │   ├── api.py              # Google Drive API wrapper
│   │   ├── uploader.py         # Upload logica
│   │   └── folder_browser.py   # Map navigatie
│   ├── ui/
│   │   ├── tray.py             # System tray icoon
│   │   ├── settings_window.py  # Instellingen venster
│   │   └── notifications.py    # Toast notificaties
│   ├── hotkey/
│   │   ├── listener.py         # Globale hotkey listener
│   │   └── handler.py          # Hotkey acties
│   └── shell/
│       ├── context_menu.py     # Context menu registratie
│       └── registry.py         # Windows registry operaties
├── resources/
│   ├── icons/                  # Applicatie iconen
│   └── credentials.json        # Google API credentials (template)
├── tests/
│   └── ...                     # Unit tests
├── installer/
│   └── ...                     # Installer scripts
├── requirements.txt
├── setup.py
└── README.md
```

## User Stories

### US-01: Bestand uploaden via context menu
**Als** gebruiker
**Wil ik** een bestand kunnen uploaden naar Google Drive via rechtsklikken
**Zodat** ik snel bestanden kan delen zonder de browser te openen

**Acceptatiecriteria:**
- [ ] "Send 2 Drive" verschijnt in context menu bij rechtsklik op bestand
- [ ] Klikken start upload naar geconfigureerde map
- [ ] Voortgang wordt getoond
- [ ] Notificatie bij voltooiing

### US-02: Bestand uploaden via sneltoets
**Als** gebruiker
**Wil ik** een geselecteerd bestand kunnen uploaden met een sneltoets
**Zodat** ik nog sneller kan uploaden zonder te klikken

**Acceptatiecriteria:**
- [ ] Ctrl+G (of geconfigureerde toets) start upload
- [ ] Werkt wanneer bestand geselecteerd is in Verkenner
- [ ] Zelfde feedback als context menu upload

### US-03: Google Account koppelen
**Als** gebruiker
**Wil ik** mijn Google account kunnen koppelen via OAuth
**Zodat** de applicatie toegang heeft tot mijn Google Drive

**Acceptatiecriteria:**
- [ ] OAuth flow opent in standaard browser
- [ ] Na autorisatie wordt account gekoppeld
- [ ] Account info wordt getoond in instellingen
- [ ] Mogelijkheid om te ontkoppelen

### US-04: Doelmap instellen
**Als** gebruiker
**Wil ik** kunnen kiezen naar welke map bestanden worden geupload
**Zodat** mijn bestanden georganiseerd blijven

**Acceptatiecriteria:**
- [ ] Mapbrowser toont Google Drive structuur
- [ ] Geselecteerde map wordt opgeslagen
- [ ] Nieuwe map kan worden aangemaakt

### US-05: Sneltoets aanpassen
**Als** gebruiker
**Wil ik** de sneltoets kunnen aanpassen
**Zodat** ik een combinatie kan kiezen die voor mij werkt

**Acceptatiecriteria:**
- [ ] Sneltoets veld in instellingen
- [ ] Drukken van toetscombinatie registreert nieuwe sneltoets
- [ ] Waarschuwing bij conflicterende combinaties
- [ ] Reset naar standaard beschikbaar

## Roadmap

### Fase 1: MVP
- Context menu integratie
- Basis upload functionaliteit
- OAuth authenticatie
- Systeem tray applicatie

### Fase 2: Uitbreiding
- Sneltoets ondersteuning
- Instellingen venster
- Doelmap selectie
- Notificaties

### Fase 3: Polish
- Meerdere accounts
- Upload geschiedenis
- Drag & drop naar tray icoon
- Auto-update functionaliteit

## Technologie Keuzes

### Aanbevolen: Python
**Voordelen:**
- Snelle ontwikkeling
- Goede Google API libraries (`google-api-python-client`)
- PyQt6 of PySide6 voor UI
- `pystray` voor system tray
- `keyboard` of `pynput` voor globale hotkeys
- PyInstaller voor packaging

### Alternatief: Electron
**Voordelen:**
- Cross-platform (toekomstige uitbreiding)
- Moderne UI mogelijkheden
- Goede npm packages beschikbaar

**Nadelen:**
- Grotere applicatie grootte
- Meer resources nodig

## Dependencies (Python)

```
google-api-python-client>=2.0.0
google-auth-oauthlib>=1.0.0
google-auth>=2.0.0
PyQt6>=6.4.0
pystray>=0.19.0
keyboard>=0.13.5
pywin32>=305
cryptography>=40.0.0
requests>=2.28.0
```

## Configuratie Bestand Voorbeeld

```json
{
  "version": "1.0.0",
  "account": {
    "email": "user@gmail.com",
    "connected": true
  },
  "upload": {
    "default_folder_id": "1ABC123xyz",
    "default_folder_name": "Uploads",
    "show_progress": true,
    "notify_on_complete": true
  },
  "hotkey": {
    "enabled": true,
    "combination": "ctrl+g"
  },
  "general": {
    "autostart": true,
    "minimize_to_tray": true,
    "language": "nl"
  }
}
```
