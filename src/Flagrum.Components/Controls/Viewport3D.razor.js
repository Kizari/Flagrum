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
export function initialize() {
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
    controls.mouseButtons = {
        LEFT: THREE.MOUSE.PAN,
        MIDDLE: THREE.MOUSE.ROTATE,
        RIGHT: THREE.MOUSE.DOLLY
    };

    // Set up lighting
    const light = new THREE.DirectionalLight(0xffffff, 2);
    light.position.set(2, 2, 2);
    camera.add(light);
    scene.add(camera);
    scene.add(new THREE.AmbientLight(0x404040));

    // Start the render loop
    animate();
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
    
    // Set up geometry
    geometry.setAttribute("position", new THREE.Float32BufferAttribute(vertices, 3));
    geometry.setIndex(indices);
    geometry.setAttribute("normal", new THREE.Float32BufferAttribute(normals, 3));
    geometry.setAttribute("uv", new THREE.Float32BufferAttribute(uvs, 2));
    
    // Set up material
    let material;
    if (diffuse) {
        const diffuseBlob = new Blob([diffuse], {type: "image/png"});
        const url = URL.createObjectURL(diffuseBlob);
        const textureLoader = new THREE.TextureLoader();
        const normalBlob = new Blob([normalMap], {type: "image/png"});
        const normalUrl = URL.createObjectURL(normalBlob);
        textureLoader.load(url, texture => {
            texture.colorSpace = THREE.SRGBColorSpace;
            textureLoader.load(normalUrl, normalTexture => {
                material = new THREE.MeshStandardMaterial({
                    map: texture,
                    normalMap: normalTexture
                });
                const mesh = new THREE.Mesh(geometry, material);
                scene.add(mesh);
                currentMeshes.push(mesh);
                URL.revokeObjectURL(url);
                URL.revokeObjectURL(normalUrl);
            });
        });
    } else {
        material = new THREE.MeshStandardMaterial({color: 0xcccccc});
        const mesh = new THREE.Mesh(geometry, material);
        scene.add(mesh);
        currentMeshes.push(mesh);
    }
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
    const box = new THREE.Box3(
        new THREE.Vector3(minX, minY, minZ),
        new THREE.Vector3(maxX, maxY, maxZ)
    );

    const size = box.getSize(new THREE.Vector3());
    const center = box.getCenter(new THREE.Vector3());
    const maxSize = Math.max(size.x, size.y, size.z);
    const fitHeightDistance = maxSize / (2 * Math.tan(THREE.MathUtils.degToRad(camera.fov / 2)));
    const fitWidthDistance = fitHeightDistance / camera.aspect;
    const distance = Math.max(fitHeightDistance, fitWidthDistance);

    const direction = new THREE.Vector3(0, 0, 1); // Default viewing direction (front-on)
    camera.position.copy(center).add(direction.multiplyScalar(distance));
    camera.near = distance / 100;
    camera.far = distance * 100;
    camera.updateProjectionMatrix();

    if (controls) {
        controls.target.copy(center);
        controls.update();
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