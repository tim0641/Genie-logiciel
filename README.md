# EasyLib Backup Service

## Description
EasyLib Backup Service est une application de sauvegarde qui permet de créer, gérer et restaurer des sauvegardes de fichiers et de répertoires. Elle utilise des services de journalisation et de gestion des états pour suivre les opérations de sauvegarde.

## Fonctionnalités
- **Créer des sauvegardes** : Créez des sauvegardes complètes ou incrémentielles de vos fichiers et répertoires.
- **Lister les sauvegardes** : Affichez la liste des sauvegardes existantes.
- **Exécuter des sauvegardes** : Lancez des sauvegardes manuellement.
- **Supprimer des sauvegardes** : Supprimez des sauvegardes existantes.
- **Journalisation** : Suivi des opérations de sauvegarde avec des journaux quotidiens.
- **Gestion des états** : Suivi de l'état des sauvegardes en cours.

## Installation
1. Clonez le dépôt :
    ```sh
    git clone https://github.com/votre-utilisateur/Genie-logiciel.git
    ```
2. Ouvrez le projet dans Visual Studio Code ou votre IDE préféré.
3. Assurez-vous d'avoir .NET 9.0 installé sur votre machine.

## Utilisation
1. Lancez l'application en exécutant le projet `EasyCLI`.
2. Utilisez l'interface graphique pour créer, lister, exécuter et supprimer des sauvegardes.

## Structure du Projet
- **EasyLib** : Contient la logique principale de l'application, y compris les services de sauvegarde et de journalisation.
- **EasyCLI** : Contient l'interface console pour interagir avec l'application.
- **EasySaveLog** : Contient les modèles et services de journalisation.