# Location Heat Map – .NET MAUI (Android)

A .NET MAUI Android application that integrates **Google Maps SDK** to visualize geographical location points as a route-style heat path.

This project demonstrates Google Maps integration, Android API key authorization, SHA1 configuration, and runtime debugging in a real-world mobile app scenario.

---

## Features

- Google Maps integration (Maps SDK for Android)
- Dynamic location marker plotting
- Route-style dotted path visualization
- Real-time location tracking
- Secure API key restriction (Package name + SHA1)
- Android Emulator testing (API 34)

---

## Tech Stack

- **.NET MAUI**
- **C#**
- **Google Maps SDK for Android**
- **Android Emulator (Google Play image)**
- **Google Cloud Console**

---


---

## Google Maps Setup

### Create Google Cloud Project

- Go to: https://console.cloud.google.com/
- Create a new project
- Enable **Billing**
- Enable **Maps SDK for Android**

---

### Generate API Key

- Go to **APIs & Services → Credentials**
- Create API key
- Restrict the key:

**Application Restriction:**
- Android apps
- Add:
  - Package name: `com.companyname.locationheatmap`
  - SHA1 fingerprint (Debug keystore)

**API Restriction:**
- Restrict to:
  - `Maps SDK for Android`

---

### Add API Key to AndroidManifest.xml

```xml
<meta-data
    android:name="com.google.android.geo.API_KEY"
    android:value="YOUR_API_KEY_HERE" />
```

File location:
Platforms/Android/AndroidManifest.xml

---
### Getting SHA1 (Debug)

Run this command (Windows):

keytool -list -v -keystore %USERPROFILE%\.android\debug.keystore -alias androiddebugkey -st

---

### Required Android Permissions
```
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### Running the Project

Open in Visual Studio 2022

Select:

Debug | Pixel 7 – API 34 (Google Play)

Build & Run

