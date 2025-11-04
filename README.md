# Blazor Azure Services Hub

A comprehensive Blazor Server application for managing and visualizing Azure services, converted from HTML pages to interactive Blazor components with smooth user interactions and modern UI.

## 🚀 Features

### ✨ Main Pages
- **Azure Services Dashboard** - Complete catalog of Azure services organized by categories with search functionality
- **My Frequent Services** - Personalized list of most-used Azure resources with management capabilities
- **Table View** - Tabular display of Azure services for better data management
- **Branching Strategy Diagram** - Visual Git/Bitbucket branching workflow guide

### 🎨 UI/UX Enhancements
- **Responsive Design** - Works perfectly on desktop, tablet, and mobile devices
- **Interactive Components** - Smooth animations and transitions
- **Modern Styling** - Azure-themed color scheme with professional appearance
- **Easy Navigation** - Intuitive sidebar navigation with clear icons
- **Search & Filter** - Real-time search across all service categories

### 🛠️ Interactive Features
- **Add/Remove Resources** - Manage your favorite Azure services
- **Collapsible Sections** - Organized content with expandable categories
- **Modal Dialogs** - User-friendly forms for adding new resources
- **External Links** - Direct integration with Azure Portal
- **Status Indicators** - Visual service status with color coding

## 📁 Project Structure

```
BlazorAzureServices/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor          # Main application layout
│   │   └── NavMenu.razor            # Navigation sidebar
│   └── Pages/
│       ├── AzureServices.razor      # Main Azure services catalog
│       ├── MyFrequentServices.razor # Personalized services list
│       ├── MyFrequentServicesTable.razor # Table view
│       └── BitbucketBranchingDiagram.razor # Git workflow diagram
├── wwwroot/
│   ├── app.css                      # Main application styles
│   └── azure-services.css          # Azure-specific styling
└── Program.cs                       # Application entry point
```

## 🔧 Technology Stack

- **Frontend**: Blazor Server (.NET 8)
- **Styling**: Bootstrap 5 + Custom CSS
- **Icons**: Open Iconic
- **Responsive**: Mobile-first design approach

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Installation & Running

1. **Navigate to project directory**:
   ```bash
   cd "C:\Mukesh\Code\BlazorAzureServices"
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Open in browser**:
   - Navigate to: `http://localhost:5013`

## 🌐 Available Pages

| Page | Route | Description |
|------|-------|-------------|
| Azure Services | `/azure-services` | Main dashboard with service categories |
| My Services | `/my-frequent-services` | Personal services management |
| Table View | `/my-frequent-services-table` | Tabular data display |
| Git Workflow | `/bitbucket-branching-diagram` | Branching strategy guide |

## 🎯 Key Features Breakdown

### Azure Services Dashboard
- **Service Categories**: Compute, Storage, Database, Networking, AI/ML, Security, DevOps, Analytics, Management
- **Search Functionality**: Real-time filtering across all services
- **Statistics Display**: Total services and categories counter
- **Quick Actions**: Direct links to Azure Portal, documentation, and personal lists

### My Frequent Services
- **Personal Management**: Add/remove frequently used services
- **Service Sections**: Organized by service type (Functions, Storage, Databases, etc.)
- **Status Monitoring**: Visual indicators for service health
- **Tag Support**: Environment tags (prod, dev, test, uat)
- **Interactive Forms**: Modal dialogs for adding new resources

### Table View
- **Sortable Columns**: Click headers to sort data
- **Expandable Sections**: Collapse/expand service categories
- **Quick Actions**: View, edit, and manage services
- **Responsive Tables**: Mobile-optimized table display

### Git Branching Diagram
- **Visual Workflow**: Interactive branching strategy diagram
- **Best Practices**: Git flow guidelines and commands
- **Reference Cards**: Quick lookup for branch types
- **Command Examples**: Copy-paste Git commands

## 🎨 Styling & Themes

### Color Scheme
- **Primary Blue**: `#0078d4` (Azure brand color)
- **Success Green**: `#28a745`
- **Warning Orange**: `#f39c12`
- **Danger Red**: `#e74c3c`
- **Light Background**: `#f5f5f5`

### Typography
- **Primary Font**: Segoe UI, Tahoma, Geneva, Verdana, sans-serif
- **Code Font**: 'Courier New', monospace

## 📱 Responsive Design

The application is fully responsive and optimized for:
- **Desktop**: Full-featured experience with sidebar navigation
- **Tablet**: Adapted layout with touch-friendly controls  
- **Mobile**: Optimized for small screens with collapsible navigation

## 🔧 Customization

### Adding New Services
1. Navigate to the relevant page component
2. Add new service items to the initialization methods
3. Include appropriate icons, descriptions, and links

### Modifying Styles
- Edit `wwwroot/app.css` for global styles
- Modify component-specific CSS within individual `.razor` files
- Update color schemes in CSS variables

### Extending Functionality
- Add new pages in `Components/Pages/`
- Update navigation in `NavMenu.razor`
- Implement new service categories in data models

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Converted from original HTML/CSS/JavaScript implementation
- Azure branding and color scheme inspiration from Microsoft Azure
- Icons provided by Open Iconic and Unicode emojis
- Bootstrap framework for responsive grid system

---

**Built with ❤️ for Azure developers and administrators**