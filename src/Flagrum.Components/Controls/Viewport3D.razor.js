import THREE from "/_content/Flagrum.Components/three.bundle.mjs";

let container, scene, camera, renderer, controls;
let currentMeshes = [];

/** Used for observing changes in the size of the viewport container. */
const resizeObserver = new ResizeObserver(_ => {
    onResize();
});

/**
 * Initializes the ThreeJS scene for the 3D viewer.
 */
export function initialize(left, middle, right) {
    container = document.getElementById("Viewport3DContainer");
    resizeObserver.observe(container);

    // Create the scene
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x44403C);

    // Create a renderer
    renderer = new THREE.WebGLRenderer({
        antialias: true
    });
    renderer.domElement.style.width = "100%";
    renderer.domElement.style.height = "100%";
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    container.appendChild(renderer.domElement);

    // Create a camera
    const aspectRatio = container.offsetWidth / container.offsetHeight;
    camera = new THREE.PerspectiveCamera(75, aspectRatio, 0.1, 1000);
    camera.position.set(0, 5, 5);
    camera.lookAt(0, 1, 0);
    onResize();

    // Create controls for rotating, zooming, and panning
    controls = new THREE.OrbitControls(camera, renderer.domElement);
    controls.mouseButtons = {};
    if (left > -1) {controls.mouseButtons.LEFT = left;}
    if (middle > -1) {controls.mouseButtons.MIDDLE = middle;}
    if (right > -1) {controls.mouseButtons.RIGHT = right;}

    // Set up lighting
    const light = new THREE.DirectionalLight(0xffffff, 2);
    light.position.set(2, 2, 2);
    camera.add(light);
    scene.add(camera);
    scene.add(new THREE.AmbientLight(0x404040));

    // Start the render loop
    animate();
}

export function setLeftClick(action) {
    controls.mouseButtons.LEFT = action > -1 ? action : undefined;
}

export function setMiddleClick(action) {
    controls.mouseButtons.MIDDLE = action > -1 ? action : undefined;
}

export function setRightClick(action) {
    controls.mouseButtons.RIGHT = action > -1 ? action : undefined;
}

/**
 * Adds a mesh to the scene.
 *
 * @param vertices 1D array of vertex positions (X, Y, Z, X, Y, Z ...)
 * @param indices 1D array of vertex indices for triangle faces.
 * @param normals 1D array of per-vertex normals (X, Y, Z, X, Y, Z ...)
 * @param uvs 1D array of per-vertex UVs (U, V, U, V, ...)
 * @param diffuse Byte array containing PNG data for the diffuse texture.
 * @param normalMap Byte array containing PNG data for the normal map.
 */
export function addMesh(vertices, indices, normals, uvs, diffuse, normalMap) {
    const geometry = new THREE.BufferGeometry();
    const textureLoader = new THREE.TextureLoader();
    const promises = [];
    const materialParams = {};

    // Set up geometry
    geometry.setAttribute("position", new THREE.Float32BufferAttribute(vertices, 3));
    geometry.setIndex(indices);
    geometry.setAttribute("normal", new THREE.Float32BufferAttribute(normals, 3));
    geometry.setAttribute("uv", new THREE.Float32BufferAttribute(uvs, 2));

    // Load diffuse texture
    if (diffuse) {
        const diffuseBlob = new Blob([diffuse], {type: "image/png"});
        const diffuseUrl = URL.createObjectURL(diffuseBlob);
        promises.push(new Promise(resolve => {
            textureLoader.load(diffuseUrl, texture => {
                texture.colorSpace = THREE.SRGBColorSpace;
                materialParams.map = texture;
                URL.revokeObjectURL(diffuseUrl);
                resolve();
            });
        }));
    }

    // Load normal texture
    if (normalMap) {
        const normalBlob = new Blob([normalMap], {type: "image/png"});
        const normalUrl = URL.createObjectURL(normalBlob);
        promises.push(new Promise(resolve => {
            textureLoader.load(normalUrl, texture => {
                materialParams.normalMap = texture;
                URL.revokeObjectURL(normalUrl);
                resolve();
            });
        }));
    }

    // Wait for texture loading to complete
    Promise.all(promises).then(() => {
        // Create the material
        const material = Object.keys(materialParams).length > 0
            ? new THREE.MeshStandardMaterial(materialParams)
            : new THREE.MeshStandardMaterial({color: 0xcccccc});

        // Add the mesh to the scene
        const mesh = new THREE.Mesh(geometry, material);
        scene.add(mesh);
        currentMeshes.push(mesh);
    });
}

/**
 * Removes all meshes from the scene and disposes them.
 */
export function clearMeshes() {
    if (currentMeshes) {
        currentMeshes.forEach(mesh => {
            scene.remove(mesh);
            mesh.geometry.dispose();
            mesh.material.dispose();
        });

        currentMeshes = [];
    }
}

/**
 * Sets the camera up so that it nicely frames the model within the viewport.
 * @param minX X value of the minimum corner of the model's bounding box.
 * @param minY Y value of the minimum corner of the model's bounding box.
 * @param minZ Z value of the minimum corner of the model's bounding box.
 * @param maxX X value of the maximum corner of the model's bounding box.
 * @param maxY Y value of the maximum corner of the model's bounding box.
 * @param maxZ Z value of the maximum corner of the model's bounding box.
 */
export function frameModel(minX, minY, minZ, maxX, maxY, maxZ) {
    // Compute bounds
    const box = new THREE.Box3(
        new THREE.Vector3(minX, minY, minZ),
        new THREE.Vector3(maxX, maxY, maxZ)
    );

    const size = box.getSize(new THREE.Vector3());
    const center = box.getCenter(new THREE.Vector3());
    const maxSize = Math.max(size.x, size.y, size.z);
    
    // Compute minimum camera distance that fits the model with padding
    const padding = 1.3; // 30% padding
    const fitHeightDistance = (maxSize * padding) / (2 * Math.tan(THREE.MathUtils.degToRad(camera.fov / 2)));
    const fitWidthDistance = fitHeightDistance / camera.aspect;
    const distance = Math.max(fitHeightDistance, fitWidthDistance);

    // Compute cinematic camera angle
    const yaw = THREE.MathUtils.degToRad(30);
    const pitch = THREE.MathUtils.degToRad(20);
    const direction = new THREE.Vector3(0, 0, 1) // Default viewing direction (front-on)
        .applyAxisAngle(new THREE.Vector3(0, 1, 0), yaw) // Rotate slightly to viewer's left
        .applyAxisAngle(new THREE.Vector3(1, 0, 0), -pitch) // Tilt downward slightly
        .normalize();
    
    // Apply fit and angle to camera
    camera.position.copy(center).add(direction.multiplyScalar(distance));
    camera.near = distance / 100;
    camera.far = distance * 100;
    camera.updateProjectionMatrix();
    
    // Offset target to compensate for perspective
    const targetOffset = new THREE.Vector3(
        size.x * 0.02,   // Visually shift model slightly left
        size.y * 0.02,   // Visually shift model slightly downward
        0
    );

    const adjustedTarget = center.clone().add(targetOffset);

    // Apply final camera transformation
    if (controls) {
        controls.target.copy(adjustedTarget);
        controls.update();
    } else {
        camera.lookAt(adjustedTarget);
    }
}

/**
 * Cleans up all resources when done with the viewer.
 */
export function dispose() {
    clearMeshes();
    renderer.dispose();
}

/**
 * Slowly rotates the model each frame.
 */
export function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}

/**
 * Updates the renderer size and the camera projection when the viewport container changes size.
 */
function onResize() {
    if (container.offsetHeight === 0) {
        return;
    }

    camera.aspect = container.offsetWidth / container.offsetHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(container.offsetWidth, container.offsetHeight, false);
}