import React, { useState, useEffect } from "react";
import PageWrapper from "./routes/PageWrapper";
import AdminWrapper from "./routes/AdminWrapper";

// layouts
import { Layout } from "./layouts/Layout";

// components
import ErrorModal from "./components/modals/ErrorModal";
import LicenseErrorModal from "./components/modals/LicenseErrorModal";

// styles
import "./custom.css";
import "./bootstrap.css"; /* needed for the carousel control */

// help
import { Help } from './pages/help/Home';
import { Scanning } from './pages/help/Scanning';
import { ApiIntegrations } from './pages/help/ApiIntegrations';

// pages
import { NotFound } from "./pages/NotFound";
import { AccessDenied } from "./pages/AccessDenied";
import { Account } from "./pages/Account";
import { Home } from "./pages/Home";
import { Login } from "./pages/Login";
import Inventory from "./pages/Inventory";
import Search from "./pages/Search";
import Boms from "./pages/Boms";
import Bom from "./pages/Bom";
import Project from "./pages/Project";
import { Datasheets } from "./pages/Datasheets";
import LowInventory from "./pages/LowInventory";
import { OrderImport } from "./pages/OrderImport";
import { PartTypes } from "./pages/PartTypes";
import { ExportData } from "./pages/ExportData";
import { Printing } from "./pages/printing/Home";
import { PrintLabels } from "./pages/printing/PrintLabels";
import { PrintLabels2 } from "./pages/printing/PrintLabels2";
import { BulkPrint } from "./pages/printing/BulkPrint";
import { Tools } from "./pages/Tools";
import { Settings } from "./pages/Settings";
import { OhmsLawCalculator } from "./pages/tools/OhmsLawCalculator";
import { ResistorColorCodeCalculator } from "./pages/tools/ResistorColorCodeCalculator";
import { SmdResistorCodeCalculator } from "./pages/tools/SmdResistorCodeCalculator";
import { VoltageDividerCalculator } from "./pages/tools/VoltageDividerCalculator";
import { BarcodeScanner } from "./pages/tools/BarcodeScanner";
import { KeyboardDump } from "./pages/tools/KeyboardDump";
import { BOM } from "./pages/help/BOM";

// admin
import { Users } from "./pages/admin/users/Users";
import { User } from "./pages/admin/users/User";
import { Admin } from "./pages/admin/Home";
import { Backup } from "./pages/admin/Backup";
import { UpdateParts } from "./pages/admin/UpdateParts";
import { ActivateLicense } from "./pages/admin/ActivateLicense";
import { SystemInformation } from "./pages/admin/SystemInformation";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '*',
    element: <NotFound />
  },

  /* Public routes */
  {
    path: '/accessdenied',
    element: <AccessDenied />
  },
  {
    path: '/login',
    element: <Login />
  },
  {
    path: '/account',
    element: <Account />
  },
  {
    path: 'inventory/:partNumber',
    element: <Inventory />
  },
  {
    path: 'inventory/add/:partNumberToAdd?',
    element: <Inventory />
  },
  {
    path: 'inventory',
    element: <Search />
  },
  {
    path: 'project/:project',
    element: <Project />
  },
  {
    path: 'bom',
    element: <Boms />
  },
  {
    path: 'bom/:project',
    element: <Bom />
  },
  {
    path: 'datasheets',
    element: <Datasheets />
  },
  {
    path: 'lowstock',
    element: <LowInventory />
  },
  {
    path: 'import',
    element: <OrderImport />
  },
  {
    path: 'partTypes',
    element: <PartTypes />
  },
  {
    path: 'projects',
    element: <Bom />
  },
  {
    path: 'exportData',
    element: <ExportData />
  },
  {
    path: 'printing',
    element: <Printing />
  },
  {
    path: 'printing/printLabels',
    element: <PrintLabels />
  },
  {
    path: 'printing/labelTemplates',
    element: <PrintLabels2 />
  },
  {
    path: 'printing/bulkPrint',
    element: <BulkPrint />
  },
  {
    path: 'tools',
    element: <Tools />
  },
  {
    path: 'tools/ohmslaw',
    element: <OhmsLawCalculator />
  },
  {
    path: 'tools/resistor',
    element: <ResistorColorCodeCalculator />
  },
  {
    path: 'tools/smdresistor',
    element: <SmdResistorCodeCalculator />
  },
  {
    path: 'tools/voltagedivider',
    element: <VoltageDividerCalculator />
  },
  {
    path: 'tools/barcodescanner',
    element: <BarcodeScanner />
  },
  {
    path: 'tools/keyboarddump',
    element: <KeyboardDump />
  },
  {
    path: 'settings',
    element: <Settings />
  },
  {
    path: 'help',
    element: <Help />
  },
  {
    path: 'help/scanning',
    element: <Scanning />
  },
  {
    path: 'help/api-integrations',
    element: <ApiIntegrations />
  },
  {
    path: 'help/bom',
    element: <BOM />
  },

  /* ADMIN ROUTES */
  {
    path: 'admin',
    element: <AdminWrapper><Admin /></AdminWrapper>
  },
  {
    path: 'admin/users',
    element: <AdminWrapper><Users /></AdminWrapper>
  },
  {
    path: 'admin/users/:userId',
    element: <AdminWrapper><User /></AdminWrapper>
  },
  {
    path: 'admin/backup',
    element: <AdminWrapper><Backup /></AdminWrapper>
  },
  {
    path: 'admin/info',
    element: <AdminWrapper><SystemInformation /></AdminWrapper>
  },
  {
    path: 'admin/updateParts',
    element: <AdminWrapper><UpdateParts /></AdminWrapper>
  },
  {
    path: 'admin/activateLicense',
    element: <AdminWrapper><ActivateLicense /></AdminWrapper>
  }
];

export default AppRoutes;
