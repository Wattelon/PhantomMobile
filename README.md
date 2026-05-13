# PhantomMobile – AR visualization for medical ultrasound phantoms

[![Lab Website](https://img.shields.io/badge/MUSL-Lab_Website-blue)](https://drleonov.github.io)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3+-black)](https://unity.com)

**PhantomMobile** is an open‑source mobile augmented reality (AR) application developed by the [Moscow UltraSound Laboratory (MUSL)](https://drleonov.github.io). It enables real‑time visualization of multimodal medical images (CT, MRI, ultrasound) over physical anthropomorphic phantoms.

<!-- 
<p align="center">
  <img src="Docs/screenshot_demo.png" alt="PhantomMobile AR demo" width="300">
  <br>
  <em>Example: AR overlay on a thyroid phantom (screenshot placeholder – replace with actual image).</em>
</p>
 -->

## 🎯 Purpose

- Provide an accessible AR tool for **medical education and radiology training**.
- Support **multimodal image fusion** (anatomical atlas, CT/MRI/ ultrasound slices) on low‑cost Android devices.
- Enable **reproducible research** in ultrasound phantoms and AR‑guided interventions.

## ✨ Features

- Recognises physical phantoms (thyroid, head, abdomen) via Vuforia / OpenXR.
- Displays:
  - 3D anatomical atlas overlay
  - Real‑time alignment of CT, MRI, and ultrasound slices
  - Multi‑planar reconstruction (axial, sagittal, coronal)
- Supports DICOM series upload and synchronised cross‑hair navigation.
- Built with **Unity 2022.3** and **OpenXR** for cross‑platform compatibility.

## 🧪 Compatible phantoms

- Anthropomorphic thyroid phantom (main test object)
- Head phantom (transcranial ultrasound studies)
- Abdominal phantom (liver / kidney training)

> If you plan to use PhantomMobile with your own phantoms, please [contact us](mailto:leonovd.v@ya.ru) – we can help adapt the target recognition.

## 📲 Getting started

### Prerequisites

- Unity 2022.3 (or newer) with Android Build Support
- Android device (ARCore‑compatible recommended) or HoloLens 2
- Git LFS (for large assets)

### Build from source

1. Clone the repository:
   ```bash
   git clone https://github.com/Wattelon/PhantomMobile.git
Open the project in Unity.

Configure your Android/iOS/HoloLens settings.

Build and run on your target device.

Detailed build instructions will be added as the project matures. For now, feel free to experiment – and please open an issue if you encounter problems.

📖 How to cite
If you use PhantomMobile in your research, please cite the MosMedAR platform and the relevant MUSL publications:

text
MosMedAR Project Team (2026). MosMedAR: Augmented Reality for Medical Education and Imaging.
Moscow UltraSound Laboratory. https://drleonov.github.io/ar/
For scientific background, refer to our papers on anthropomorphic phantoms and AR navigation (see MUSL Publications).

🤝 Contributing
We welcome contributions! Feel free to:

Report bugs or suggest features via GitHub Issues

Submit pull requests for improvements or new features

Please read our contribution guidelines (if you add one) before submitting.

📜 License
This project is licensed under the MIT License – see the LICENSE file for details.

📬 Contact
Laboratory website: https://drleonov.github.io
