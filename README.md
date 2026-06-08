# Jojk

## Welcome to the Avalonia based music player!

This application is built using Avalonia and aims to provide an intuitive and seamless music listening experience, primarily made to replace the regular Jellyfin web player.

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
  - [.NET SDK Installation](#net-sdk-installation)
  - [Git Installation](#git-installation)
- [Installation](#installation)
- [Usage](#usage)
- [Development](#development)
  - [Fork the Repository](#fork-the-repository)
  - [Set Up the Environment](#set-up-the-environment)
  - [Create a New Branch](#create-a-new-branch)
  - [Make Changes and Commit](#make-changes-and-commit)
  - [Push to Your Fork](#push-to-your-fork)
  - [Open a Pull Request](#open-a-pull-request)
- [Contributing](#contributing)
- [License](#license)

## Overview

Jojk is a desktop application designed to enhance your music listening experience. It leverages the power of Avalonia for cross-platform compatibility, ensuring that you can enjoy your music on any device with minimal configuration.

## Features

- **Cross-platform Support**: Since it's built using Avalonia, Jojk can be ported to Linux and macOS quite simply (although it is primarily made for Windows as of now). Plans to make it more cross-platform are in the works.
- **Integration with Jellyfin**: Seamlessly connects with Jellyfin servers to provide a unified music library and playback experience.
- **User-friendly Interface**: Clean and intuitive UI designed for ease of use and customization.
- **Efficient Performance**: Small, lightweight, simple calls. This also has plans for further improvements as time goes on.

## Prerequisites

Before getting started, ensure that you have the following installed on your machine:

### .NET SDK Installation

1. Download the latest version of the [.NET SDK](https://dotnet.microsoft.com/download).
2. Follow the installation instructions provided by the official documentation.

### Git Installation

1. Download and install [Git](https://git-scm.com/).
2. Configure Git with your username and email:

```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## Installation

You can install Jojk directly from source. Here’s how you can do it:

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/BusterLandstrom/Jojk.git
   ```

2. **Navigate to the Project Directory**:

   ```bash
   cd Jojk
   ```

3. **Restore Dependencies**:

   ```bash
   dotnet restore
   ```

4. **Build the Project**:

   ```bash
   dotnet build -c Release
   ```

5. **Run the Application**:

   ```bash
   dotnet run -c Release --project Jojk.sln
   ```

## Usage

Here are some basic instructions on how to use Jojk:

1. **Launch the Application**: Run the executable from the build output directory or use the `dotnet` command as shown above.
2. **Connect to Jellyfin Server**:
   - Open the settings menu.
   - Enter your Jellyfin server details (URL, port, username, and password).
3. **Explore Your Library**: Use the navigation pane to browse through your music albums, artists, and genres.
4. **Play Music**: Select a track and click play or use the media controls to manage playback.

## Development 

*This part is not fully set in stone yet, as I will allow changes only if necessary since it's so small. Contributions are currently not something I am interested in.*

If you are interested in contributing to Jojk or developing it further, follow these steps:

### Fork the Repository

- Click the "Fork" button on GitHub and clone your forked repository locally.

   ```bash
   git clone https://github.com/your-username/Jojk.git
   ```

### Set Up the Environment

- Follow the Installation instructions to set up the development environment.

### Create a New Branch

   ```bash
   git checkout -b feature/new-feature-name
   ```

### Make Changes and Commit

   ```bash
   git add .
   git commit -m "Add new feature"
   ```

### Push to Your Fork

   ```bash
   git push origin feature/new-feature-name
   ```

### Open a Pull Request

- Go to the original repository on GitHub and create a pull request from your branch.

## Contributing

We welcome contributions! Please ensure that your code adheres to the following guidelines:

- Follow the coding style guide for consistent formatting.
- Write clear and concise commit messages.
- Include documentation updates where necessary.
- Test your changes thoroughly before submitting a pull request.

For any issues or suggestions, please open an issue on GitHub.

## License

Jojk is released under the MIT License. See the [LICENSE](LICENSE) file for more information.