import { BrowserRouter } from "react-router-dom";
import Approutes from "./routes/Approutes";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Approutes />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
