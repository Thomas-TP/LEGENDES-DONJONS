# LEGENDES & DONJONS 🛡️🐉

```
  _      ______ _____ ______ _   _ _____  ______  _____      _    
 | |    |  ____/ ____|  ____| \ | |  __ \|  ____|/ ____|    | |   
 | |    | |__ | |  __| |__  |  \| | |  | | |__  | (___      | |   
 | |    |  __|| | |_ |  __| | . ` | |  | |  __|  \___ \     | |   
 | |____| |___| |__| | |____| |\  | |__| | |____ ____) |    |_|   
 |______|______\_____|______|_| \_|_____/|______|_____/     (_)   
                                                                  
  _____   ____  _   _    _  ____  _   _  _____                    
 |  __ \ / __ \| \ | |  | |/ __ \| \ | |/ ____|                   
 | |  | | |  | |  \| |  | | |  | |  \| | (___                     
 | |  | | |  | | . ` |_ | | |  | | . ` |\___ \                    
 | |__| | |__| | |\  |__| | |__| | |\  |____) |                   
 |_____/ \____/|_| \_\____|\____/|_| \_|_____/                    
```

## 📜 Description

**LEGENDES & DONJONS** est un jeu de rôle (RPG) immersif développé en C# .NET 8, proposant une expérience riche en stratégie et en aventure. Explorez des donjons mystérieux, affrontez un bestiaire varié et faites évoluer votre héros légendaire.

Le projet se décline en deux expériences :
1.  **Version Console** : Une aventure textuelle rétro sublimée par [Spectre.Console](https://spectreconsole.net/).
2.  **Version Web** : Une interface moderne et réactive construite avec React, Vite et TailwindCSS, propulsée par une API ASP.NET Core robuste.

### ✨ Fonctionnalités Clés
*   **Classes de Personnages** : Incarnez un Guerrier robuste 🛡️, un Mage puissant 🔮, ou un Archer agile 🏹.
*   **Système de Combat** : Combats au tour par tour tactiques avec gestion des dégâts, de la santé et des capacités spéciales.
*   **Bestiaire Étendu** : Affrontez des Gobelins, des Dragons, des Squelettes et bien d'autres créatures mythiques.
*   **Progression** : Gagnez de l'expérience, montez de niveau et débloquez de nouvelles compétences.
*   **Boutique & Inventaire** : Achetez de l'équipement, des potions et gérez votre inventaire pour survivre.
*   **Sauvegarde** : Système de persistance des données (JSON) pour ne jamais perdre votre progression.

## 🏰 Architecture Technique

Le projet suit les principes de l'architecture logicielle moderne et du Clean Code.

### 🧠 Backend (C# .NET 8)
L'architecture est modulaire et sépare clairement les responsabilités :

*   **Domain (`src/JeuDeRole/JeuDeRole/Domain`)** : Le cœur du métier. Contient les Entités (`Entities`), les Objets de Valeur (`ValueObjects`), et les Modèles (`Models`) qui définissent les règles du jeu.
*   **Services (`src/JeuDeRole/JeuDeRole/Services`)** : La logique applicative (ex: `GameSessionService`, `CombatService`). Orchestre les interactions entre les entités.
*   **Factories (`src/JeuDeRole/JeuDeRole/Factories`)** : Design Pattern Factory pour la création dynamique de monstres et de personnages.
*   **Strategies (`src/JeuDeRole/JeuDeRole/Strategies`)** : Design Pattern Strategy pour varier les comportements (ex: IA des monstres).
*   **Repositories (`src/JeuDeRole/JeuDeRole/Repositories`)** : Abstraction de l'accès aux données (actuellement `InMemory` et fichiers JSON).

### 🌐 Frontend (React + Vite)
L'interface web est située dans `src/JeuDeRole.Web/ClientApp` :
*   **Framework** : React 18 avec TypeScript pour la robustesse du typage.
*   **Build Tool** : Vite pour des temps de démarrage ultra-rapides.
*   **Styling** : TailwindCSS pour un design utilitaire et réactif.
*   **Communication** : Appels API REST vers le backend ASP.NET Core.

### 📐 Schéma d'Architecture Simplifié

```mermaid
graph TD
    Client[Client Web (React)] -->|HTTP API| WebAPI[ASP.NET Core Web API]
    Console[Console App] -->|Direct Call| CoreLogic
    WebAPI --> CoreLogic[Core Logic (Services/Domain)]
    CoreLogic --> Data[Data Access (JSON/Memory)]
    
    subgraph "Core Logic"
    Services --> Domain
    Services --> Factories
    Factories --> Domain
    end
```

## 🚀 Installation et Démarrage

### Prérequis
*   [**.NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [**Node.js**](https://nodejs.org/) (version LTS recommandée)

### 1. Cloner le dépôt
```bash
git clone https://github.com/Thomas-TP/LEGENDES-DONJONS.git
cd LEGENDES-DONJONS
```

### 2. Lancer la Version Console 🖥️
Pour une expérience rétro immédiate :

```bash
cd src/JeuDeRole/JeuDeRole
dotnet run
```

### 3. Lancer la Version Web 🌐

**Backend (API) :**
Ouvrez un terminal à la racine du projet :
```bash
cd src/JeuDeRole.Web
dotnet run
```
L'API démarrera (par défaut sur `http://localhost:5xxx`).

**Frontend (Client React) :**
Ouvrez un *nouveau* terminal :
```bash
cd src/JeuDeRole.Web/ClientApp
npm install
npm run dev
```
Ouvrez votre navigateur sur l'adresse indiquée (généralement `http://localhost:5173`).

---

## 🛠️ Commandes Utiles

*   **Exécuter les tests** :
    ```bash
    dotnet test tests/JeuDeRole.Tests/JeuDeRole.Tests/JeuDeRole.Tests.csproj
    ```

## 📧 Contact & Support

Projet maintenu par **Thomas-TP**.
Pour toute question ou suggestion, n'hésitez pas à ouvrir une Issue sur GitHub.

---
*Fait avec ❤️ et beaucoup de café ☕*
