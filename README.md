# PaL.X - Plateforme de Communication Sécurisée

PaL.X est une solution complète de messagerie instantanée, d'appels vocaux et vidéo, conçue avec **.NET 9**. Elle se compose d'un client lourd (WinForms), d'une interface d'administration et d'une API robuste.

![PaL.X Banner](https://via.placeholder.com/800x200?text=PaL.X+Communication+Platform)

## 🚀 Fonctionnalités Principales

### 💬 Messagerie Instantanée
*   Chat en temps réel via **SignalR**.
*   Support du texte riche (RTF), emojis et envoi de fichiers.
*   Historique des conversations persistant (PostgreSQL).
*   Statuts utilisateur (En ligne, Absent, Ne pas déranger, etc.).

### 📹 Appels Vidéo & Audio
*   **Nouveau :** Appels vidéo haute qualité via **WebRTC** (intégré via WebView2).
*   Appels vocaux fluides.
*   Interface d'appel moderne (Dark Theme) avec gestion Caméra/Micro.
*   Signalisation P2P sécurisée.

### 🛡️ Administration
*   Dashboard de gestion des utilisateurs et des sessions.
*   Contrôle du service (Démarrage/Arrêt du backend).
*   Logs et surveillance de l'activité en temps réel.

---

## 🛠️ Stack Technique

*   **Backend** : ASP.NET Core 9.0, Entity Framework Core, SignalR.
*   **Frontend** : Windows Forms (.NET 9), WebView2 (pour WebRTC).
*   **Base de données** : PostgreSQL.
*   **Protocoles** : HTTPS (5001), WSS (Secure WebSocket), WebRTC.

---

## ⚙️ Prérequis

1.  **SDK .NET 9.0** installé.
2.  **PostgreSQL** (v13 ou supérieur) en cours d'exécution.
3.  **WebView2 Runtime** (généralement préinstallé sur Windows 10/11).

---

## 🔧 Installation et Configuration

### 1. Base de Données
Créez une base de données vide nommée `PaL.X` dans PostgreSQL.
La chaîne de connexion par défaut est configurée pour un utilisateur `postgres` avec le mot de passe `2012704`.
*Pour modifier cela, éditez `src/PaL.X.Api/appsettings.Development.json`.*

### 2. Certificat HTTPS
Le projet utilise désormais exclusivement HTTPS sur le port **5001**. Assurez-vous de faire confiance au certificat de développement :
```powershell
dotnet dev-certs https --trust
```

### 3. Démarrage Rapide
Un script est disponible à la racine pour lancer l'environnement complet :
```bat
start_all.bat
```
*Cela lancera l'API, le Client et l'Admin.*

---

## ▶️ Démarrage Manuel

### API (Backend)
L'API écoute sur `https://localhost:5001` et `http://localhost:5000`.
```bash
cd src/PaL.X.Api
dotnet run --launch-profile https
```

### Interface Admin
Permet de gérer le service.
```bash
cd src/PaL.X.Admin
dotnet run
```
*Note : Vous pouvez démarrer/arrêter le backend directement depuis l'écran de login de l'Admin.*

### Client Utilisateur
```bash
cd src/PaL.X.Client
dotnet run
```

---

## 🔍 Dépannage

**L'API ne démarre pas (Erreur DB)**
*   Vérifiez que le service PostgreSQL est lancé.
*   Vérifiez le mot de passe dans `appsettings.Development.json`.

**Warning HTTPS au démarrage**
*   Si vous voyez "Failed to determine the https port", assurez-vous d'avoir exécuté `dotnet dev-certs https --trust`.

**Écran noir en appel vidéo**
*   Vérifiez que vous avez autorisé l'accès Caméra/Micro si Windows le demande.
*   Assurez-vous que le runtime WebView2 est à jour.

---

## 👥 Auteurs
*   **DeLTa-X-Tunisia** - *Développement Principal*

---
*Projet développé sous .NET 9 - 2025*
