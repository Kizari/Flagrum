# Flagrum.Components

Razor components for the Flagrum application.

## 3D Viewer

This component renders 3D content via ThreeJS, which uses WebGL.
ThreeJS is provided in `wwwroot/three.bundle.mjs`. To make this bundle (such as if updating), the following steps
can be follows.

1. Create an empty directory and open a console in it
2. `npm install three`
3. `npm install --save-dev vite`
4. Create `vite.config.js` (see below)
5. Create `main.js` (see below)
6. `npx vite build`
7. Copy the new `three.bundle.mjs` from `dist` into the `wwwroot` of this project

**vite.config.js**
```javascript
export default {
    build: {
        lib: {
            entry: 'main.js',
            name: 'Three',
            fileName: 'three.bundle',
            formats: ['es']
        },
        outDir: './dist'
    }
};
```

**main.js**
```javascript
import * as THREE from "three";
import {OrbitControls} from "three/examples/jsm/controls/OrbitControls.js";

const ThreeExtended = {
    ...THREE,
    OrbitControls
};

export default ThreeExtended;
```