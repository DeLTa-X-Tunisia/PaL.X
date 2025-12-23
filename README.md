# PaL.X - Plateforme de Communication Unifiée

PaL.X est une suite de communication complète et moderne développée en **.NET 9**. Elle offre une expérience utilisateur riche combinant messagerie instantanée, partage multimédia et appels vidéo haute définition, le tout sécurisé par une architecture robuste.

![PaL.X Banner](https://via.placeholder.com/800x200?text=PaL.X+Communication+Platform)

## 🌟 Fonctionnalités Détaillées

### 💬 Messagerie & Chat Complet
Une expérience de chat fluide et interactive :
*   **Messagerie Instantanée** : Échanges en temps réel ultra-rapides via **SignalR**.
*   **Smileys & Émojis** : Support étendu de packs de smileys (Basic, Premium, Animés) pour enrichir les conversations.
*   **Mise en forme** : Support du texte riche (couleurs, polices, styles).
*   **Statuts de Présence** : Gestion dynamique des statuts (En ligne, Occupé, Absent, Invisible).

### 📂 Partage Multimédia Avancé
PaL.X va au-delà du simple texte :
*   **Transfert de Fichiers** : Envoi et réception de tout type de documents avec barre de progression.
*   **Partage d'Images** : Prévisualisation et envoi rapide de photos directement dans le chat.
*   **Messages Audio** : Enregistrement vocal intégré et lecteur audio natif pour envoyer des notes vocales.

### 📹 Appels Vidéo & Audio (WebRTC)
Communication en temps réel de nouvelle génération :
*   **Technologie WebRTC** : Appels vidéo P2P haute qualité et faible latence (via WebView2).
*   **Interface Moderne** : Fenêtre d'appel "Dark Theme" immersive.
*   **Contrôles Complets** : Gestion du micro, de la caméra et bascule plein écran.
*   **Menu Contextuel** : Lancement rapide d'appels depuis la liste d'amis.

### 🛡️ Confidentialité & Gestion des Contacts
Un contrôle total sur vos interactions :
*   **Système d'Amis** : Recherche, demande d'ajout et gestion de la liste de contacts.
*   **Système de Blocage Avancé** : 
    *   Bloquez les utilisateurs indésirables pour empêcher tout contact (messages ou appels).
    *   Gestionnaire de liste noire (Blacklist) accessible depuis les paramètres.
    *   Protection immédiate de la vie privée.

### 🔧 Administration Système
Un panneau de contrôle puissant pour les administrateurs :
*   **Dashboard** : Vue d'ensemble des utilisateurs connectés et de l'état du serveur.
*   **Contrôle de Service** : Démarrage et arrêt du backend API à la demande.
*   **Logs Système** : Suivi des événements et diagnostics en temps réel.

---

## 🛠️ Architecture Technique

Le projet repose sur une stack technologique de pointe :

*   **Core Framework** : .NET 9.0 (Dernière version LTS).
*   **Backend API** : ASP.NET Core Web API.
*   **Communication** : SignalR (WebSocket) & WebRTC (Vidéo).
*   **Client Desktop** : Windows Forms (WinForms) modernisé.
*   **Base de Données** : PostgreSQL avec Entity Framework Core.
*   **Sécurité** : Authentification JWT, HTTPS (Port 5001).

---

## ⚙️ Prérequis

Pour exécuter PaL.X, assurez-vous d'avoir :
1.  **SDK .NET 9.0** installé.
2.  **PostgreSQL** (v13+) en cours d'exécution.
3.  **WebView2 Runtime** (Standard sur Windows 10/11).

---

## 🚀 Installation Rapide

1.  **Base de Données** :
    Créez une base vide PaL.X dans PostgreSQL.
    *(Config par défaut : User postgres / Pass 2012704)*

2.  **Démarrage** :
    Utilisez le script start_all.bat à la racine pour lancer l'environnement complet (API + Client + Admin).

3.  **Premier Login** :
    Créez un compte via l'interface client ou utilisez les comptes de test si générés.

---

*Développé avec passion sur .NET 9.*
