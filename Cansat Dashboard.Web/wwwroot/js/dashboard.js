let scene, camera, renderer, cansat;

window.init3DModel = () => {
    const container = document.getElementById('model-container');
    if (!container) {
        console.error("Container for 3D model not found.");
        return;
    }

    // Clear placeholder
    const placeholder = document.getElementById('model-placeholder');
    if (placeholder) {
        placeholder.remove();
    }

    // Scene setup
    scene = new THREE.Scene();
    camera = new THREE.PerspectiveCamera(75, container.clientWidth / container.clientHeight, 0.1, 1000);
    renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(container.clientWidth, container.clientHeight);
    container.appendChild(renderer.domElement);

    // Lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.8);
    scene.add(ambientLight);
    const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(5, 10, 7.5);
    scene.add(directionalLight);

    // Load custom OBJ model
    const loader = new THREE.OBJLoader();
    loader.load(
        '/models/cansat.obj', // Path to your obj file
        (object) => {
            cansat = object;
            scene.add(cansat);
        },
        (xhr) => {
            console.log((xhr.loaded / xhr.total * 100) + '% loaded');
        },
        (error) => {
            console.error('An error happened while loading the model:', error);
        }
    );

    camera.position.z = 3;

    function animate() {
        requestAnimationFrame(animate);
        renderer.render(scene, camera);
    }
    animate();

    // Handle container resize
    new ResizeObserver(() => {
        const width = container.clientWidth;
        const height = container.clientHeight;
        camera.aspect = width / height;
        camera.updateProjectionMatrix();
        renderer.setSize(width, height);
    }).observe(container);
};

window.updateRotation = (pitch, roll, yaw) => {
    if (cansat) {
        cansat.rotation.x = pitch * (Math.PI / 180);
        cansat.rotation.z = roll * (Math.PI / 180);
        cansat.rotation.y = yaw * (Math.PI / 180);
    }
};

window.dispose3DModel = () => {
    // Clean up Three.js resources
    // (Implementation depends on the complexity of the scene)
};