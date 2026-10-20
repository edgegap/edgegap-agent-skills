import { listen } from "@colyseus/tools";
import app from "./app.config";

// Colyseus Cloud sets PORT; locally it's 2567.
listen(app);
